using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ajedrez
{
    internal enum PieceType { Pawn, Rook, Knight, Bishop, Queen, King }

    internal class LightPiece
    {
        public PieceType Type;
        public int Color; // 1 white, 0 black
        public int Row;
        public int Col;
        public bool HasMoved;
        public bool HasJustMovedTwo;

        public LightPiece(PieceType type, int color, int row, int col)
        {
            Type = type;
            Color = color;
            Row = row;
            Col = col;
            HasMoved = false;
            HasJustMovedTwo = false;
        }

        public LightPiece Clone() => new LightPiece(this.Type, this.Color, this.Row, this.Col)
        {
            HasMoved = this.HasMoved,
            HasJustMovedTwo = this.HasJustMovedTwo
        };
    }

    internal class BoardModel
    {
        public int Rows { get; } = 8;
        public int Cols { get; } = 8;
        private LightPiece?[,] grid;

        public BoardModel()
        {
            grid = new LightPiece?[Rows, Cols];
        }

        public LightPiece? Get(int r, int c) => (r >= 0 && r < Rows && c >= 0 && c < Cols) ? grid[r, c] : null;
        public void Set(int r, int c, LightPiece? p)
        {
            if (r >= 0 && r < Rows && c >= 0 && c < Cols) grid[r, c] = p;
        }

        public BoardModel Clone()
        {
            var copy = new BoardModel();
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    copy.grid[r, c] = grid[r, c]?.Clone();
            return copy;
        }

        public static BoardModel FromUniformGrid(UniformGrid board)
        {
            var m = new BoardModel();
            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Columns; c++)
                {
                    var border = board.Children[r * board.Columns + c] as Border;
                    if (border?.Child is System.Windows.Controls.Image img && img.Tag is Piece p)
                    {
                        PieceType t = p switch
                        {
                            Pawn _ => PieceType.Pawn,
                            Rook _ => PieceType.Rook,
                            Knight _ => PieceType.Knight,
                            Bishop _ => PieceType.Bishop,
                            Queen _ => PieceType.Queen,
                            King _ => PieceType.King,
                            _ => PieceType.Pawn
                        };
                        var lp = new LightPiece(t, p.Color, p.Position.Item1, p.Position.Item2)
                        {
                            HasMoved = p.hasMoved
                        };
                        m.Set(r, c, lp);
                    }
                }
            }
            return m;
        }

        // Apply move on model. Handles capture and basic en-passant and promotion to queen.
        public void ApplyMove(LightPiece piece, int toRow, int toCol)
        {
            int fromRow = piece.Row;
            int fromCol = piece.Col;

            // reset just moved two flags
            foreach (var lp in EnumeratePieces()) lp.HasJustMovedTwo = false;

            // en-passant capture: if pawn moves diagonally to empty square, capture pawn behind
            if (piece.Type == PieceType.Pawn && Math.Abs(toCol - fromCol) == 1 && Get(toRow, toCol) == null)
            {
                var ep = Get(fromRow, toCol);
                if (ep != null && ep.Type == PieceType.Pawn && ep.Color != piece.Color && ep.HasJustMovedTwo)
                {
                    Set(fromRow, toCol, null);
                }
            }

            // capture / move
            Set(toRow, toCol, piece);
            Set(fromRow, fromCol, null);
            piece.Row = toRow;
            piece.Col = toCol;
            // mark moved
            piece.HasMoved = true;

            // if pawn moved two squares, mark
            if (piece.Type == PieceType.Pawn && Math.Abs(toRow - fromRow) == 2)
            {
                piece.HasJustMovedTwo = true;
            }

            // promotion - replace type with queen
            if (piece.Type == PieceType.Pawn)
            {
                if ((piece.Color == 1 && piece.Row == 0) || (piece.Color == 0 && piece.Row == Rows - 1))
                {
                    piece.Type = PieceType.Queen;
                }
            }
        }

        public IEnumerable<LightPiece> EnumeratePieces()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (grid[r, c] != null) yield return grid[r, c]!;
        }

        // Generate pseudolegal moves for a lightpiece
        public List<(int r, int c)> GenerateMoves(LightPiece piece)
        {
            var moves = new List<(int r, int c)>();
            int r = piece.Row, c = piece.Col;
            int dir = piece.Color == 1 ? -1 : 1;
            switch (piece.Type)
            {
                case PieceType.Pawn:
                    // forward
                    if (Get(r + dir, c) == null) { moves.Add((r + dir, c)); if (!piece.HasMoved && Get(r + 2*dir, c) == null) moves.Add((r + 2*dir, c)); }
                    // captures
                    foreach (int dc in new[] { -1, 1 })
                    {
                        var target = Get(r + dir, c + dc);
                        if (target != null && target.Color != piece.Color) moves.Add((r + dir, c + dc));
                        // en-passant
                        var adj = Get(r, c + dc);
                        if (adj != null && adj.Type == PieceType.Pawn && adj.Color != piece.Color && adj.HasJustMovedTwo)
                        {
                            moves.Add((r + dir, c + dc));
                        }
                    }
                    break;
                case PieceType.Knight:
                    foreach (var dcdr in new[] { (-2,-1),(-1,-2),(-2,1),(-1,2),(1,-2),(2,-1),(2,1),(1,2) })
                    {
                        int nr = r + dcdr.Item1, nc = c + dcdr.Item2;
                        if (nr>=0 && nr<Rows && nc>=0 && nc<Cols)
                        {
                            var t = Get(nr,nc);
                            if (t==null || t.Color!=piece.Color) moves.Add((nr,nc));
                        }
                    }
                    break;
                case PieceType.Bishop:
                case PieceType.Rook:
                case PieceType.Queen:
                    var directions = new List<(int dr,int dc)>();
                    if (piece.Type == PieceType.Bishop || piece.Type == PieceType.Queen) directions.AddRange(new[]{(-1,-1),(-1,1),(1,-1),(1,1)});
                    if (piece.Type == PieceType.Rook || piece.Type == PieceType.Queen) directions.AddRange(new[]{(-1,0),(1,0),(0,-1),(0,1)});
                    foreach (var (dr,dc) in directions)
                    {
                        int nr = r+dr, nc = c+dc;
                        while (nr>=0 && nr<Rows && nc>=0 && nc<Cols)
                        {
                            var t = Get(nr,nc);
                            if (t==null) { moves.Add((nr,nc)); }
                            else { if (t.Color!=piece.Color) moves.Add((nr,nc)); break; }
                            nr+=dr; nc+=dc;
                        }
                    }
                    break;
                case PieceType.King:
                    foreach (var d in new[]{(-1,0),(1,0),(0,1),(0,-1),(-1,-1),(-1,1),(1,-1),(1,1)})
                    {
                        int nr=r+d.Item1,nc=c+d.Item2;
                        if (nr>=0 && nr<Rows && nc>=0 && nc<Cols)
                        {
                            var t = Get(nr,nc);
                            if (t==null || t.Color!=piece.Color) moves.Add((nr,nc));
                        }
                    }
                    break;
            }
            return moves;
        }

        // Check if king of given color is in check
        public bool IsKingInCheck(int kingColor)
        {
            LightPiece? king = EnumeratePieces().FirstOrDefault(p => p.Type == PieceType.King && p.Color == kingColor);
            if (king == null) return false;
            var kingPos = (king.Row, king.Col);
            foreach (var p in EnumeratePieces())
            {
                if (p.Color == kingColor) continue;
                var moves = GenerateMoves(p);
                if (moves.Any(m => m.r == kingPos.Item1 && m.c == kingPos.Item2)) return true;
            }
            return false;
        }
    }
}
