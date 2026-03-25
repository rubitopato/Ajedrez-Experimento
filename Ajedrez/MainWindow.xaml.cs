using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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