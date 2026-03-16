using System;
using System.Collections.Generic;
using System.Text;
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
                        img.Tag = ps; // o asigna un nuevo objeto modelo si quieres manipularlo
                        border.Child = img;
                    }
                    grid.Children.Add(border);
                }
            return grid;
        }
    }
}
