using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Ajedrez
{
    public partial class PromotionWindow : Window
    {
        public string SelectedPieceType { get; private set; } = string.Empty;
        private int color;
        private string asm;

        public PromotionWindow(int color, string asmName)
        {
            InitializeComponent();
            this.color = color;
            this.asm = asmName;

            string colorName = this.color == 1 ? "blanco" : "negro";
            try
            {
                QueenButton.Content = CreatePieceImage($"pack://application:,,,/{this.asm};component/Images/reina_{colorName}.png");
                RookButton.Content = CreatePieceImage($"pack://application:,,,/{this.asm};component/Images/torre_{colorName}.png");
                BishopButton.Content = CreatePieceImage($"pack://application:,,,/{this.asm};component/Images/alfil_{colorName}.png");
                KnightButton.Content = CreatePieceImage($"pack://application:,,,/{this.asm};component/Images/caballo_{colorName}.png");
            }
            catch
            {
                QueenButton.Content = "Queen";
                RookButton.Content = "Rook";
                BishopButton.Content = "Bishop";
                KnightButton.Content = "Knight";
            }
        }

        private Image CreatePieceImage(string uri)
        {
            var img = new Image
            {
                Source = new BitmapImage(new Uri(uri, UriKind.Absolute)),
                Width = 48,
                Height = 64,
                Stretch = Stretch.Uniform
            };
            return img;
        }

        private void QueenButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPieceType = "Queen";
            this.DialogResult = true;
            this.Close();
        }

        private void RookButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPieceType = "Rook";
            this.DialogResult = true;
            this.Close();
        }

        private void BishopButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPieceType = "Bishop";
            this.DialogResult = true;
            this.Close();
        }

        private void KnightButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPieceType = "Knight";
            this.DialogResult = true;
            this.Close();
        }
    }
}