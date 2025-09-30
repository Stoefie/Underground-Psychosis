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

namespace Underground_Psychosis
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // this way you don't have to assign it via XAML.
            StartButton.Click += StartButton_Click;
            OptionsButton.Click += OptionsButton_Click;
            ExitButton.Click += ExitButton_Click;
        }



        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Start clicked!");
            
        }

        // redirect can be added for new 'options' page.
        public void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Options clicked!");

        }

        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Exit clicked!");

        }
    }
}