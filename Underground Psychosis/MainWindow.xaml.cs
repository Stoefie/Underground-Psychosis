using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Underground_Psychosis.Levels;

namespace Underground_Psychosis
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = new MainMenu();

            
            try
            {
                var potionImg = new BitmapImage(new Uri("pack://application:,,,/images/potion.png", UriKind.Absolute));
                InventoryCtrl.AddItem("Health Potion", potionImg);
            }
            catch
            {
               
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.I)
            {
                InventoryCtrl.Visibility = InventoryCtrl.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                e.Handled = true;
            }
        }
    }
}