using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Underground_Psychosis.GameEngine
{
    
    public partial class GameUI : UserControl
    {

        private DispatcherTimer? timer;
        private DateTime startTime;

        private int maxHealth = 3;
        private int currentHealth;

        public GameUI()
        {

            InitializeComponent();
            InitializeHUD();
            this.SizeChanged += (s, e) =>
            {
                UpdateHealthUI();
                UpdateTimerSize();
            };
            StartTimer();
        }

        private void InitializeHUD()
        {
            currentHealth = maxHealth;
            UpdateHealthUI();

            // Initialize timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += Timer_Tick;
        }

        // starts the timer when called
        public void StartTimer()
        {
            startTime = DateTime.Now;
            timer?.Start();
        }

        // stops timer when called
        public void StopTimer()
        {
            timer?.Stop();
        }

        // handles timer behaviour 
        private void Timer_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - startTime;
            TimerText.Text = $"{elapsed.Minutes}:{elapsed.Seconds:D2}";
        }

        // sets timer size relative to 8% of screen size
        private void UpdateTimerSize()
        {
            TimerText.FontSize = this.ActualHeight * 0.08;
        }

        // function to handle taking damage
        public void TakeDamage(int amount = 1)
        {
            currentHealth = Math.Max(0, currentHealth - amount);
            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {

            HeartsPanel.Children.Clear();

            // sets variables to make the heart size relative to 10% of screen size
            double heartSize = this.ActualHeight * 0.1; 
            double spacing = heartSize * 0.05;
            
            // adds the images and changes images depending on how many lives left
            for (int i = 0; i < maxHealth; i++)
            {
                var heart = new Image
                {
                    Width = heartSize,
                    Height = heartSize,
                    Source = new BitmapImage(new Uri(
                        i < currentHealth
                            ? "pack://application:,,,/Images/heartfull.png"
                            : "pack://application:,,,/Images/heartempty.png",
                        UriKind.Absolute)),
                    Margin = new Thickness(2)
                };
                HeartsPanel.Children.Add(heart);
            }
        }

        
    }
}
