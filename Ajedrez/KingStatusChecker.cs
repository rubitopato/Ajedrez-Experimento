using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ajedrez
{
    internal class KingStatusChecker
    {
        public static bool IsKingInCheck(Piece king, UniformGrid board)
        {
            Dictionary<Tuple<int, string>, List<Tuple<int, int>>> allContraryValidMoves = CalculateAllValidMovesForColor(king.Color == 1 ? 0 : 1, board);
            return allContraryValidMoves.Values.Any(moves => moves.Contains(king.Position));
        }

        public static string CheckKingStatus(Piece king, UniformGrid board, string asm)
        {
            bool IsCheck = IsKingInCheck(king, board);

            king.CalculateValidMoves(board);
            king.CheckInvalidMoves(board, king, asm);
            //Dictionary<Tuple<int, string>, List<Tuple<int, int>>> allContraryValidMoves = CalculateAllValidMovesForColor(king.Color == 1 ? 0 : 1, board);
            //bool hasEscape = king.ValidMoves.Any(move => !allContraryValidMoves.Values.Any(moves => moves.Contains(move)));
            bool hasEscape = king.ValidMoves.Count > 0;

            Dictionary<Tuple<int, string>, List<Tuple<int, int>>> allAlliesValidMoves = CalculateAllValidMovesForColor(king.Color, board);
            bool canBeSaved = kingCanBeSaved(king, allAlliesValidMoves, asm, board);

            if (IsCheck && (hasEscape || canBeSaved))
            {
                return "En Jaque";
            }
            else if (IsCheck && !hasEscape && !canBeSaved)
            {
                return "En Jaque Mate"; // Solo jaque
            }
            else if (!IsCheck && !hasEscape && !canBeSaved)
            {
                return "Ahogado";
            }
            else
            {
                return "A salvo"; // Esto no debería ocurrir, pero es una medida de seguridad
            }


        }

        private bool IsStalemate(int color)
        {
            // Implementa la lógica para verificar si el juego está en tablas
            return false; // Placeholder
        }

        private static bool kingCanBeSaved(Piece king, Dictionary<Tuple<int, string>, List<Tuple<int, int>>> allyMoves, string asm, UniformGrid board)
        {

            foreach (var ally in allyMoves)
            {
                if (ally.Key.Item2.Contains("King")) continue;
                foreach (var move in ally.Value)
                {
                    // Simula el movimiento
                    List<Piece> boardState = BoardGenerator.CaptureBoardState(board);
                    UniformGrid copyBoard = BoardGenerator.BuildGridFromState(8, 8, boardState, asm);
                    var border = copyBoard.Children[ally.Key.Item1] as Border;
                    var img = border?.Child as Image;
                    Piece p = img?.Tag as Piece;
                    p.Move(move, copyBoard, asm, false);
                    // Verifica si el rey sigue en jaque después del movimiento
                    if (!IsKingInCheck(king, copyBoard))
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        private static Dictionary<Tuple<int, string>, List<Tuple<int, int>>> CalculateAllValidMovesForColor(int color, UniformGrid board)
        {
            Dictionary<Tuple<int, string>, List<Tuple<int, int>>> allValidMoves = new Dictionary<Tuple<int, string>, List<Tuple<int, int>>>();
            foreach (var child in board.Children)
            {
                if (child is Border border && border.Child is Image img && img.Tag is Piece p && p.Color == color && p is not King)
                {
                    p.CalculateValidMoves(board);
                    allValidMoves.Add(Tuple.Create(p.Position.Item1 * board.Columns + p.Position.Item2, p.Name), p.ValidMoves);
                }
            }
            return allValidMoves;
        }
    }
}
