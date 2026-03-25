using System.Windows;

namespace Ajedrez
{
    /// <summary>
    /// Lógica de interacción para VictoryWindow.xaml
    /// </summary>
    public partial class VictoryWindow : Window
    {
        public VictoryWindow(string msg)
        {
            InitializeComponent();
            VictoryLabel.Content = msg;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
