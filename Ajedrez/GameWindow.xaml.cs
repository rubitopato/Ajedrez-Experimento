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

namespace Ajedrez
{
    public partial class GameWindow : Window
    {
        private Piece selectedPiece = null;
        private Piece WhiteKing = null;
        private Piece BlackKing = null;

        private static readonly string? asm = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly Dictionary<Tuple<int, int>, Piece> initialPieces = new Dictionary<Tuple<int, int>, Piece>
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

        public GameWindow()
        {
            InitializeComponent();
            DrawBoard();
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
                selectedPiece.Move(Tuple.Create(row, column), BoardGrid);
                selectedPiece.CalculateValidMoves(BoardGrid);
                selectedPiece.CheckInvalidMoves(BoardGrid, selectedPiece.Color == 1 ? WhiteKing : BlackKing, asm);

                checksChecker.Content = $"El rey {(selectedPiece.Color == 1 ? "Negro" : "Blanco")} está {KingStatusChecker.CheckKingStatus(selectedPiece.Color == 1 ? BlackKing : WhiteKing, BoardGrid, asm)}!";

                if (selectedPiece.Color == 1)
                {
                    SetPiecesClickableByColor(1, false);
                    SetPiecesClickableByColor(0, true);
                }
                else
                {
                    SetPiecesClickableByColor(1, true);
                    SetPiecesClickableByColor(0, false);
                }
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
    }
}