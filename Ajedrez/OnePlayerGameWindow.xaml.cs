using System.Collections;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;
using Ajedrez.Engine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Ajedrez
{
    public partial class OnePlayerGameWindow : Window
    {
        private StockfishEngine? engine = null;
        private string? enginePath = null; // path to stockfish executable

        // Media player for playing sounds from resources
        private MediaPlayer mediaPlayer = new MediaPlayer();

        private Piece selectedPiece = null;
        private Piece WhiteKing = null;
        private Piece BlackKing = null;
        private int RedBorder = 0;

        private static readonly string? asm = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        private readonly Dictionary<Tuple<int, int>, Piece> initialPieces = new Dictionary<Tuple<int, int>, Piece>
            {
                { Tuple.Create(0, 0), new Rook("Black_Rook_0", Tuple.Create(0, 0), $"pack://application:,,,/{asm};component/Images/torre_negro.png") },
                { Tuple.Create(0, 1), new Knight("Black_Knight_0", Tuple.Create(0, 1), $"pack://application:,,,/{asm};component/Images/caballo_negro.png") },
                { Tuple.Create(0, 2), new Bishop("Black_Bishop_0", Tuple.Create(0, 2), $"pack://application:,,,/{asm};component/Images/alfil_negro.png") },
                { Tuple.Create(0, 3), new Queen("Black_Queen", Tuple.Create(0, 3), $"pack://application:,,,/{asm};component/Images/reina_negro.png") },
                { Tuple.Create(0, 4), new King("Black_King", Tuple.Create(0, 4), $"pack://application:,,,/{asm};component/Images/rey_negro.png") },
                { Tuple.Create(0, 5), new Bishop("Black_Bishop_1", Tuple.Create(0, 5), $"pack://application:,,,/{asm};component/Images/alfil_negro.png") },
                { Tuple.Create(0, 6), new Knight("Black_Knight_1", Tuple.Create(0, 6), $"pack://application:,,,/{asm};component/Images/caballo_negro.png") },
                { Tuple.Create(0, 7), new Rook("Black_Rook_1", Tuple.Create(0, 7), $"pack://application:,,,/{asm};component/Images/torre_negro.png") },
                { Tuple.Create(1, 0), new Pawn("Black_Pawn_0", Tuple.Create(1, 0), $"pack://application:,,,/{asm};component/Images/peon_negro.png") },
                { Tuple.Create(1, 1), new Pawn("Black_Pawn_1", Tuple.Create(1, 1), $"pack://application:,,,/{asm};component/Images/peon_negro.png") },
                { Tuple.Create(1, 2), new Pawn("Black_Pawn_2", Tuple.Create(1, 2), $"pack://application:,,,/{asm};component/Images/peon_negro.png") },
                { Tuple.Create(1, 3), new Pawn("Black_Pawn_3", Tuple.Create(1, 3), $"pack://application:,,,/{asm};component/Images/peon_negro.png") },
                { Tuple.Create(1, 4), new Pawn("Black_Pawn_4", Tuple.Create(1, 4), $"pack://application:,,,/{asm};component/Images/peon_negro.png") },
                { Tuple.Create(1, 5), new Pawn("Black_Pawn_5", Tuple.Create(1, 5), $"pack://application:,,,/{asm};component/Images/peon_negro.png") },
                { Tuple.Create(1, 6), new Pawn("Black_Pawn_6", Tuple.Create(1, 6), $"pack://application:,,,/{asm};component/Images/peon_negro.png") },
                { Tuple.Create(1, 7), new Pawn("Black_Pawn_7", Tuple.Create(1, 7), $"pack://application:,,,/{asm};component/Images/peon_negro.png") },
                { Tuple.Create(7, 0), new Rook("White_Rook_0", Tuple.Create(7, 0), $"pack://application:,,,/{asm};component/Images/torre_blanco.png") },
                { Tuple.Create(7, 1), new Knight("White_Knight_0", Tuple.Create(7, 1), $"pack://application:,,,/{asm};component/Images/caballo_blanco.png") },
                { Tuple.Create(7, 2), new Bishop("White_Bishop_0", Tuple.Create(7, 2), $"pack://application:,,,/{asm};component/Images/alfil_blanco.png") },
                { Tuple.Create(7, 3), new Queen("White_Queen", Tuple.Create(7, 3), $"pack://application:,,,/{asm};component/Images/reina_blanco.png") },
                { Tuple.Create(7, 4), new King("White_King", Tuple.Create(7, 4), $"pack://application:,,,/{asm};component/Images/rey_blanco.png") },
                { Tuple.Create(7, 5), new Bishop("White_Bishop_1", Tuple.Create(7, 5), $"pack://application:,,,/{asm};component/Images/alfil_blanco.png") },
                { Tuple.Create(7, 6), new Knight("White_Knight_1", Tuple.Create(7, 6), $"pack://application:,,,/{asm};component/Images/caballo_blanco.png") },
                { Tuple.Create(7, 7), new Rook("White_Rook_1", Tuple.Create(7, 7), $"pack://application:,,,/{asm};component/Images/torre_blanco.png") },
                { Tuple.Create(6, 0), new Pawn("White_Pawn_0", Tuple.Create(6, 0), $"pack://application:,,,/{asm};component/Images/peon_blanco.png") },
                { Tuple.Create(6, 1), new Pawn("White_Pawn_1", Tuple.Create(6, 1), $"pack://application:,,,/{asm};component/Images/peon_blanco.png") },
                { Tuple.Create(6, 2), new Pawn("White_Pawn_2", Tuple.Create(6, 2), $"pack://application:,,,/{asm};component/Images/peon_blanco.png") },
                { Tuple.Create(6, 3), new Pawn("White_Pawn_3", Tuple.Create(6, 3), $"pack://application:,,,/{asm};component/Images/peon_blanco.png") },
                { Tuple.Create(6, 4), new Pawn("White_Pawn_4", Tuple.Create(6, 4), $"pack://application:,,,/{asm};component/Images/peon_blanco.png") },
                { Tuple.Create(6, 5), new Pawn("White_Pawn_5", Tuple.Create(6, 5), $"pack://application:,,,/{asm};component/Images/peon_blanco.png") },
                { Tuple.Create(6, 6), new Pawn("White_Pawn_6", Tuple.Create(6, 6), $"pack://application:,,,/{asm};component/Images/peon_blanco.png") },
                { Tuple.Create(6, 7), new Pawn("White_Pawn_7", Tuple.Create(6, 7), $"pack://application:,,,/{asm};component/Images/peon_blanco.png") },
            };

        public OnePlayerGameWindow()
        {
            InitializeComponent();
            DrawBoard();

            // attempt to locate engine at ./Engines/stockfish.exe
            var tryPath = System.IO.Path.Combine("C:\\Users\\conno\\source\\repos\\Ajedrez\\Ajedrez\\Engines\\", "stockfish.exe");
            if (File.Exists(tryPath))
            {
                enginePath = tryPath;
            }
        }

        public void StartStockfish(string exePath)
        {
            try
            {
                StopStockfish();
                engine = new StockfishEngine();
                engine.Start(exePath);
                enginePath = exePath;
            }
            catch (Exception ex)
            {
                engine = null;
                enginePath = null;
                MessageBox.Show($"No se pudo iniciar Stockfish: {ex.Message}");
            }
        }

        public void StopStockfish()
        {
            try
            {
                engine?.Stop();
                engine?.Dispose();
            }
            catch { }
            engine = null;
        }

        public async Task RequestEngineMoveAsync(int movetimeMs = 1000, CancellationToken ct = default)
        {
            if (engine == null || !engine.IsRunning) return;

            // generate FEN from current board; determine active color by which pieces are clickable
            // If white pieces are clickable then it's white to move, else black.

            var fen = BoardGenerator.GenerateFENFromBoard(BoardGrid, "b");
            engine.PositionFen(fen);

            try
            {
                var best = await engine.GoMovetimeAsync(movetimeMs, ct).ConfigureAwait(false);
                // apply move on UI thread
                Dispatcher.Invoke(() => ApplyUciMove(best));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Engine error: {ex.Message}"));
            }
        }

        private void ApplyUciMove(string uci)
        {
            if (string.IsNullOrEmpty(uci) || uci.Length < 4) return;
            // parse e2e4 or e7e8q
            int fromFile = uci[0] - 'a';
            int fromRank = uci[1] - '0';
            int toFile = uci[2] - 'a';
            int toRank = uci[3] - '0';

            int fromRow = BoardGrid.Rows - fromRank;
            int fromCol = fromFile;
            int toRow = BoardGrid.Rows - toRank;
            int toCol = toFile;

            var fromBorder = (Border)BoardGrid.Children[fromRow * BoardGrid.Columns + fromCol];
            var piece = Piece.GetPieceAt(fromBorder);
            if (piece == null) return;

            // if promotion
            char? prom = null;
            if (uci.Length >= 5) prom = char.ToLowerInvariant(uci[4]);

            // move without showing promotion UI
            piece.Move(Tuple.Create(toRow, toCol), BoardGrid, asm, false);

            if (prom.HasValue && piece is Pawn)
            {
                string colorName = piece.Color == 1 ? "blanco" : "negro";
                string colorPrefix = piece.Color == 1 ? "White" : "Black";
                string index = "0";
                Piece newPiece = prom.Value switch
                {
                    'q' => new Queen($"{colorPrefix}_Queen_{index}", piece.Position, $"pack://application:,,,/{asm};component/Images/reina_{colorName}.png"),
                    'r' => new Rook($"{colorPrefix}_Rook_{index}", piece.Position, $"pack://application:,,,/{asm};component/Images/torre_{colorName}.png"),
                    'b' => new Bishop($"{colorPrefix}_Bishop_{index}", piece.Position, $"pack://application:,,,/{asm};component/Images/alfil_{colorName}.png"),
                    'n' => new Knight($"{colorPrefix}_Knight_{index}", piece.Position, $"pack://application:,,,/{asm};component/Images/caballo_{colorName}.png"),
                    _ => new Queen($"{colorPrefix}_Queen_{index}", piece.Position, $"pack://application:,,,/{asm};component/Images/reina_{colorName}.png"),
                };
                int idx = piece.Position.Item1 * BoardGrid.Columns + piece.Position.Item2;
                ((Border)BoardGrid.Children[idx]).Child = newPiece.ImageControl;
            }

            // update kings references if moved
            if (piece.Name.Contains("White_King")) WhiteKing = piece;
            else if (piece.Name.Contains("Black_King")) BlackKing = piece;

            // After engine (black) move, enable white pieces and disable black pieces
            SetPiecesClickableByColor(1, true);
            changePawnsThatMovedTwo(BoardGrid, 1);

            PlaySound();

            int status = KingStatusChecker.CheckKingStatus(WhiteKing, BoardGrid, asm);
            switch (status)
            {
                case 0:
                    Border cb = ((Border)BoardGrid.Children[RedBorder]);
                    cb?.Background = (string)cb.Tag == "Light" ? Brushes.Bisque : Brushes.SaddleBrown;
                    break;
                case 1:
                    Tuple<int, int> kingPos = WhiteKing.Position;
                    RedBorder = kingPos.Item1 * BoardGrid.Columns + kingPos.Item2;
                    ((Border)BoardGrid.Children[RedBorder]).Background = Brushes.Red;
                    break;
                case 2:
                    var win = new VictoryWindow("Los negros ganan");
                    var result = win.ShowDialog();
                    if (result == true) { Reset(); } else { this.Close(); }
                    break;
                case 3:
                    var drawWin = new VictoryWindow("Empate: Rey blanco ahogado");
                    var drawResult = drawWin.ShowDialog();
                    if (drawResult == true) { Reset(); } else { this.Close(); }
                    break;
            }
        }

        private void DrawBoard()
        {

            if (BoardGrid == null) return;

            BoardGrid.Children.Clear();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var isLight = (row + col) % 2 == 0;
                    var border = new Border
                    {
                        Background = isLight ? Brushes.Bisque : Brushes.SaddleBrown,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Tag = isLight ? "Light" : "Dark"
                    };

                    Piece piece = BorderInitializesWithPiece(row, col);

                    if (piece != null)
                    {
                        border.Child = piece.ImageControl;

                        if (piece.Name == "White_King")
                        {
                            WhiteKing = piece;
                        }
                        else if (piece.Name == "Black_King")
                        {
                            BlackKing = piece;
                        }
                    }
                    border.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler((s, e) => BorderLeftMouseClick(s, e)), true);

                    BoardGrid.Children.Add(border);
                }
            }

            SetPiecesClickableByColor(1, true);
            SetPiecesClickableByColor(0, false);
        }

        private Piece BorderInitializesWithPiece(int row, int col)
        {
            if (initialPieces.TryGetValue(Tuple.Create(row, col), out var piece))
            {
                return piece;
            }
            else
            {
                return null;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            var w = new OnePlayerGameWindow();
            w.Show();
            this.Close();
        }

        private void PieceImage_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (sender is Image img && img.Tag is Piece piece)
            {
                ShowValidMoves(BoardGrid, piece);
            }
        }
        private void ShowValidMoves(UniformGrid board, Piece piece)
        {
            if (selectedPiece != piece && selectedPiece != null)
            {
                foreach (var move in selectedPiece.ValidMoves)
                {
                    int index = move.Item1 * board.Columns + move.Item2;
                    var border = board.Children[index] as Border;
                    if (border != null)
                    {
                        border.Background = (string)border.Tag == "Light" ? Brushes.Bisque : Brushes.SaddleBrown;
                    }
                }
            }
            selectedPiece = piece;
            selectedPiece.CalculateValidMoves(board);
            selectedPiece.CheckInvalidMoves(board, selectedPiece.Color == 1 ? WhiteKing : BlackKing, asm);
            foreach (var move in selectedPiece.ValidMoves)
            {
                int index = move.Item1 * board.Columns + move.Item2;
                var border = board.Children[index] as Border;
                if (border != null)
                {
                    border.Background = Brushes.LightGreen; // Resalta las casillas válidas
                }
            }
        }

        private void BorderLeftMouseClick(object? sender, MouseButtonEventArgs e)
        {
            if (selectedPiece == null) return;

            var border = sender as Border;

            int row = BoardGrid.Children.IndexOf(border) / BoardGrid.Columns;
            int column = BoardGrid.Children.IndexOf(border) % BoardGrid.Columns;

            if (selectedPiece.ValidMoves.Contains(Tuple.Create(row, column)))
            {
                foreach (var move in selectedPiece.ValidMoves)
                {
                    int index = move.Item1 * BoardGrid.Columns + move.Item2;
                    var borderAux = BoardGrid.Children[index] as Border;
                    if (borderAux != null)
                    {
                        borderAux.Background = (string)borderAux.Tag == "Light" ? Brushes.Bisque : Brushes.SaddleBrown;
                    }
                }
                selectedPiece.Move(Tuple.Create(row, column), BoardGrid, asm, true);
                PlaySound();

                int movedColor = selectedPiece.Color; // capture who moved (1 white, 0 black)

                switch (KingStatusChecker.CheckKingStatus(selectedPiece.Color == 1 ? BlackKing : WhiteKing, BoardGrid, asm))
                {
                    case 0:
                        Border cb = ((Border)BoardGrid.Children[RedBorder]);
                        cb?.Background = (string)cb.Tag == "Light" ? Brushes.Bisque : Brushes.SaddleBrown;
                        break;

                    case 1:
                        Tuple<int, int> kingPos = selectedPiece.Color == 1 ? BlackKing.Position : WhiteKing.Position;
                        RedBorder = kingPos.Item1 * BoardGrid.Columns + kingPos.Item2;
                        ((Border)BoardGrid.Children[RedBorder]).Background = Brushes.Red;
                        break;

                    case 2:
                        var win = new VictoryWindow(selectedPiece.Color == 1 ? "Los blancos ganan" : "Los negros ganan");
                        var result = win.ShowDialog();
                        if (result == true)
                        {
                            Reset();
                        }
                        else
                        {
                            this.Close();
                        }
                        break;

                    case 3:
                        var drawWin = new VictoryWindow(selectedPiece.Color == 1 ? "Empate: Rey negro ahogado" : "Empate: Rey blanco ahogado");
                        var drawResult = drawWin.ShowDialog();
                        if (drawResult == true)
                        {
                            Reset();
                        }
                        else
                        {
                            this.Close();
                        }
                        break;
                }

                SetPiecesClickableByColor(1, false);
                changePawnsThatMovedTwo(BoardGrid, 0);

                // if white just moved (human), request engine move for black
                if (movedColor == 1)
                {
                    // ensure engine started
                    if (engine == null && !string.IsNullOrEmpty(enginePath) && File.Exists(enginePath))
                    {
                        StartStockfish(enginePath);
                    }

                    // start engine move asynchronously (do not await)
                    if (engine != null && engine.IsRunning)
                    {
                        _ = RequestEngineMoveAsync(1000, CancellationToken.None);
                    }
                }

                selectedPiece = null;
            }
        }

        private void SetPiecesClickableByColor(int color, bool enabled)
        {
            foreach (var child in BoardGrid.Children)
            {
                if (child is Border border && border.Child is Image img && img.Tag is Piece p && p.Color == color)
                {
                    // siempre quitar antes para evitar duplicados
                    img.MouseLeftButtonUp -= PieceImage_MouseLeftButtonUp;

                    if (enabled)
                    {
                        img.MouseLeftButtonUp += PieceImage_MouseLeftButtonUp;
                    }
                }
            }
        }

        private void changePawnsThatMovedTwo(UniformGrid board, int color)
        {
            foreach (var child in board.Children)
            {
                if (child is Border border && border.Child is Image img && img.Tag is Pawn pawn && pawn.Color == color)
                {
                    pawn.hasJustMovedTwo = false;
                }
            }
        }

        public void PlaySound()
        {
            string filePath = "C:\\Users\\conno\\source\\repos\\Ajedrez\\Ajedrez\\Sounds\\move.wav";
            if (string.IsNullOrEmpty(filePath)) return;

            string candidate = filePath;
            if (!Path.IsPathRooted(candidate))
            {
                candidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            }

            if (!File.Exists(candidate))
            {
                MessageBox.Show($"Archivo de audio no encontrado: {filePath}\nBuscado en: {candidate}");
                return;
            }

            try
            {
                mediaPlayer.Open(new Uri(candidate, UriKind.Absolute));
                mediaPlayer.Volume = 1;
                mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo reproducir el archivo de audio: {ex.Message}");
            }
        }

        public void StopSound()
        {
            try
            {
                mediaPlayer.Stop();
            }
            catch { }
        }
    }
}