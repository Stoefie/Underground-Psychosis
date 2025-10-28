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
    public partial class MainMenu : UserControl
    {
        public MainMenu()
        {
            InitializeComponent();

            StartButton.Click += StartButton_Click;
            OptionsButton.Click += OptionsButton_Click;
            ExitButton.Click += ExitButton_Click;
            ArcadeButton.Click += ArcadeButton_Click;
            HowToButton.Click += HowToButton_Click;
        }

        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Load the first level
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
            mainWindow.MainContent.Content = new FirstLevel();
            }
        }
        public void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the options window section
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
            mainWindow.MainContent.Content = new Options();
            }
        }
        public void ArcadeButton_Click(object sender, RoutedEventArgs e)
        {
            //Arcade mode for highscore based gameplay
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
            mainWindow.MainContent.Content = new ArcadeLevel();
            }
        }
        
        // Opens the 'How to play' menu
        public void HowToButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.MainContent.Content = new HowTo();
            }
        }
        
        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the game by pressing exit
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }


    }
}
