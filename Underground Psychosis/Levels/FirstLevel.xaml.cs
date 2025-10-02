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
using Underground_Psychosis.GameEngine;

namespace Underground_Psychosis.Levels
{
    public partial class FirstLevel : UserControl
    {
        private GameLoop _game;
        private Player _player;
        public FirstLevel()
        {
            InitializeComponent();
            _game = new GameLoop(GameCanvas);
            _player = new Player { Position = new System.Windows.Point(100, 850) };
            _game.AddEntity(_player);
            _game.Start();
        }
    }
}