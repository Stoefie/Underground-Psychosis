using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Underground_Psychosis.Levels
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {
        public MainMenu()
        {
            InitializeComponent();

            StartButton.Click += StartButton_Click;
            OptionsButton.Click += OptionsButton_Click;
            ExitButton.Click += ExitButton_Click;
        }

        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //Load the first level on clicking the start button
            MainContent.Content = new FirstLevel();
        }

        // redirect can be added for new 'options' page.
        public void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Options clicked!");

        }

        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            //Close the game by pressing exit
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }
    }
}
