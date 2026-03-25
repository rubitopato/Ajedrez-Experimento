using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace Ajedrez
{
    internal class BoardGenerator
    {
        public BoardGenerator() { }

        public static List<Piece> CaptureBoardState(UniformGrid board)
        {
            var list = new List<Piece>();
            int cols = board.Columns;
            for (int i = 0; i < board.Children.Count; i++)
            {
                var border = board.Children[i] as Border;
                if (border?.Child is Image img && img.Tag is Piece p)
                {
                    var bmi = p.ImageControl.Source as BitmapImage;
                    switch (p.Name.Split('_')[1]) // Asumiendo formato "Color_Type_Index"
                    {
                        case "Pawn":
                            list.Add(new Pawn(p.Name, p.Position, bmi.UriSource.ToString()));
                            break;
                        case "Rook":
                            list.Add(new Rook(p.Name, p.Position, bmi.UriSource.ToString()));
                            break;
                        case "Knight":
                            list.Add(new Knight(p.Name, p.Position, bmi.UriSource.ToString()));
                            break;
                        case "Bishop":
                            list.Add(new Bishop(p.Name, p.Position, bmi.UriSource.ToString()));
                            break;
                        case "Queen":
                            list.Add(new Queen(p.Name, p.Position, bmi.UriSource.ToString()));
                            break;
                        case "King":
                            list.Add(new King(p.Name, p.Position, bmi.UriSource.ToString()));
                            break;
                    }
                }
            }
            return list;
        }

        public static UniformGrid BuildGridFromState(int rows, int cols, IEnumerable<Piece> pieces, string asm)
        {
            var grid = new UniformGrid { Rows = rows, Columns = cols, Width = 560, Height = 560 };
            int total = rows * cols;

            var map = pieces.ToDictionary(p => (p.Position.Item1, p.Position.Item2));
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    var border = new Border { };
                    if (map.TryGetValue((r, c), out var ps))
                    {
                        var img = new Image
                        {
                            Source = new BitmapImage(new Uri($"pack://application:,,,/{asm};component/Images/peon_negro.png")),
                            Width = 60,
                            Height = 60,
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        img.Tag = ps;
                        border.Child = img;
                    }
                    grid.Children.Add(border);
                }
            return grid;
        }

        public static string GenerateFENFromBoard(UniformGrid board, string sideToMove = "b", int halfmoveClock = 0, int fullmoveNumber = 1)
        {
            // piece placement
            var ranks = new List<string>();
            for (int r = 0; r < board.Rows; r++)
            {
                var sb = new System.Text.StringBuilder();
                int empty = 0;
                for (int c = 0; c < board.Columns; c++)
                {
                    var border = board.Children[r * board.Columns + c] as Border;
                    if (border?.Child is System.Windows.Controls.Image img && img.Tag is Piece p)
                    {
                        if (empty > 0) { sb.Append(empty); empty = 0; }
                        char symbol = 'p';
                        var typeName = p.GetType().Name; // Pawn, Rook, Knight, Bishop, Queen, King
                        switch (typeName)
                        {
                            case "Pawn": symbol = 'p'; break;
                            case "Rook": symbol = 'r'; break;
                            case "Knight": symbol = 'n'; break;
                            case "Bishop": symbol = 'b'; break;
                            case "Queen": symbol = 'q'; break;
                            case "King": symbol = 'k'; break;
                        }
                        if (p.Color == 1) symbol = char.ToUpperInvariant(symbol);
                        sb.Append(symbol);
                    }
                    else
                    {
                        empty++;
                    }
                }
                if (empty > 0) sb.Append(empty);
                ranks.Add(sb.ToString());
            }
            // ranks built from top (row 0) to bottom (row 7) which matches FEN (8..1)
            var placement = string.Join("/", ranks);

            // active color
            var active = sideToMove;

            // castling rights
            string castling = "";
            // white: king at (7,4), rooks at (7,0) and (7,7)
            bool whiteCanK = false, whiteCanQ = false, blackCanK = false, blackCanQ = false;
            // helper to get piece at
            Piece? GetPieceAtIndex(int rr, int cc)
            {
                var b = board.Children[rr * board.Columns + cc] as Border;
                return b?.Child is System.Windows.Controls.Image im && im.Tag is Piece pc ? pc : null;
            }

            var wk = GetPieceAtIndex(7, 4);
            if (wk is Piece && wk.Name.Contains("White") && !wk.hasMoved) {
                var wrK = GetPieceAtIndex(7, 7);
                if (wrK is Piece && wrK.Name.Contains("White") && !wrK.hasMoved) whiteCanK = true;
                var wrQ = GetPieceAtIndex(7, 0);
                if (wrQ is Piece && wrQ.Name.Contains("White") && !wrQ.hasMoved) whiteCanQ = true;
            }

            var bk = GetPieceAtIndex(0, 4);
            if (bk is Piece && bk.Name.Contains("Black") && !bk.hasMoved) {
                var brK = GetPieceAtIndex(0, 7);
                if (brK is Piece && brK.Name.Contains("Black") && !brK.hasMoved) blackCanK = true;
                var brQ = GetPieceAtIndex(0, 0);
                if (brQ is Piece && brQ.Name.Contains("Black") && !brQ.hasMoved) blackCanQ = true;
            }

            if (whiteCanK) castling += 'K';
            if (whiteCanQ) castling += 'Q';
            if (blackCanK) castling += 'k';
            if (blackCanQ) castling += 'q';
            if (string.IsNullOrEmpty(castling)) castling = "-";

            // en-passant target: find pawn with hasJustMovedTwo == true
            string enpassant = "-";
            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Columns; c++)
                {
                    var b = board.Children[r * board.Columns + c] as Border;
                    if (b?.Child is System.Windows.Controls.Image im && im.Tag is Pawn pw)
                    {
                        if (pw.hasJustMovedTwo)
                        {
                            int epRow = pw.Position.Item1 + (pw.Color == 1 ? 1 : -1);
                            int epCol = pw.Position.Item2;
                            if (epRow >= 0 && epRow < board.Rows)
                            {
                                char file = (char)('a' + epCol);
                                char rank = (char)('0' + (board.Rows - epRow));
                                enpassant = $"{file}{rank}";
                            }
                            goto enpass_done;
                        }
                    }
                }
            }
        enpass_done:;

            // finalize
            var fen = $"{placement} {active} {castling} {enpassant} {halfmoveClock} {fullmoveNumber}";
            return fen;
        }
    }
}
