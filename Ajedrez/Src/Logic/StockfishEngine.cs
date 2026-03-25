using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Ajedrez
{
    internal class StockfishEngine : IDisposable
    {
        private Process? process;
        private Task? readerTask;
        private CancellationTokenSource? readerCts;
        private readonly BlockingCollection<string> outputLines = new();

        private TaskCompletionSource<string>? bestMoveTcs;

        public bool IsRunning => process != null && !process.HasExited;

        public void Start(string exePath)
        {
            Stop();

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            process.Start();

            readerCts = new CancellationTokenSource();
            readerTask = Task.Run(() => ReaderLoop(process, readerCts.Token));

            // initialize UCI
            SendCommand("uci");
        }

        private async Task ReaderLoop(Process proc, CancellationToken ct)
        {
            try
            {
                var stdout = proc.StandardOutput;
                while (!ct.IsCancellationRequested && !proc.HasExited)
                {
                    var line = await stdout.ReadLineAsync().ConfigureAwait(false);
                    if (line == null) break;
                    outputLines.Add(line);

                    // detect bestmove
                    if (line.StartsWith("bestmove ", StringComparison.Ordinal))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && bestMoveTcs != null && !bestMoveTcs.Task.IsCompleted)
                        {
                            bestMoveTcs.TrySetResult(parts[1]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // swallow
            }
        }

        public void SendCommand(string cmd)
        {
            if (!IsRunning) return;
            try
            {
                process!.StandardInput.WriteLine(cmd);
                process.StandardInput.Flush();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void SetOption(string name, string value)
        {
            SendCommand($"setoption name {name} value {value}");
        }

        public void NewGame()
        {
            SendCommand("ucinewgame");
        }

        public void PositionFen(string fen)
        {
            SendCommand($"position fen {fen}");
        }

        public void PositionStartposWithMoves(string movesUci)
        {
            SendCommand($"position startpos moves {movesUci}");
        }

        public Task<string> GoMovetimeAsync(int ms, CancellationToken ct)
        {
            if (!IsRunning) throw new InvalidOperationException("Engine not running");
            bestMoveTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            SendCommand($"go movetime {ms}");

            ct.Register(() =>
            {
                try { SendCommand("stop"); } catch { }
                bestMoveTcs?.TrySetCanceled();
            });

            return bestMoveTcs.Task;
        }

        public void Stop()
        {
            try
            {
                if (process != null && !process.HasExited)
                {
                    try { SendCommand("quit"); } catch { }
                    process.Kill(true);
                }
            }
            catch { }
            try
            {
                readerCts?.Cancel();
            }
            catch { }
            process?.Dispose(); process = null;
            readerTask = null;
            readerCts = null;
            while (outputLines.TryTake(out _)) { }
        }

        public void Dispose()
        {
            Stop();
            outputLines.Dispose();
        }
    }
}
