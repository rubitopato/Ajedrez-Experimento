using System.Windows;

namespace Ajedrez
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void one_player_play_button_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new OnePlayerGameWindow();
            gameWindow.Show();
            this.Close();
        }
        private void two_player_play_button_Click(object sender, RoutedEventArgs e)
        {
            // Abrir la ventana de juego y cerrar la ventana de inicio
            var gameWindow = new GameWindow();
            gameWindow.Show();
            this.Close();
        }

    }
}