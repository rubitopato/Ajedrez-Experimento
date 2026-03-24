using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
