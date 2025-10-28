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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Underground_Psychosis.Levels
{

    public partial class HowTo : UserControl
    {
    
        // might change name as it is not coherent
        public HowTo()
        {
            InitializeComponent();
            
            BackButton.Click += BackButton_Click;
        }

        // button to go back to menu
        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.MainContent.Content = new MainMenu();
            }
        }
    }
}
