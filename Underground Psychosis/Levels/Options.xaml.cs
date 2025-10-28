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
using Underground_Psychosis;

namespace Underground_Psychosis.Levels
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : UserControl
    {
        public Options()
        {
            InitializeComponent();

            LowResButton.Click += LowResButton_Click;
            MedResButton.Click += MedResButton_Click;
            HighResButton.Click += HighResButton_Click; 
            UltraResButton.Click += UltraResButton_Click;
            BackButton.Click += BackButton_Click;
        }
        
        public void LowResButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Height = 360;
                parentWindow.Width = 640;
            }
        }

        // redirect can be added for new 'options' page.
        public void MedResButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Height = 720;
                parentWindow.Width = 1280;
            }
        }

        public void HighResButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Height = 1080;
                parentWindow.Width = 1920;
            }
        }

        public void UltraResButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                parentWindow.Height = 1440;
                parentWindow.Width = 2560;
            }
        }
        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
            mainWindow.MainContent.Content = new MainMenu();
            }
        }

        private void LowResButton_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
