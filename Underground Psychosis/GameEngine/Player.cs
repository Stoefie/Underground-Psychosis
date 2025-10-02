using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Underground_Psychosis.GameEngine
{
    public class Player : Entity
    {
        private double gravity = 600;
        private double speed = 300;
        private bool isJumping = false;
        private System.Windows.Vector velocity;
        private double groundY = 900;

        public Player()
        {
            var imageBrushPlayerOne = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/CharacterOne.png"))
            };
            var parentWindow = Application.Current.MainWindow;
            if (parentWindow != null && parentWindow.Height == 360)
                Sprite = new Rectangle { Width = 19, Height = 44, Fill = imageBrushPlayerOne };
            else if (parentWindow != null && parentWindow.Height == 720)
                Sprite = new Rectangle { Width = 38, Height = 88, Fill = imageBrushPlayerOne };
            else
            {
                Sprite = new Rectangle { Width = 57, Height = 136, Fill = imageBrushPlayerOne };
            }
        }

        public override void Update(double deltaTime)
        {
            velocity.Y += gravity * deltaTime;

            if(Keyboard.IsKeyDown(Key.A))
                velocity.X = -speed;
            else if(Keyboard.IsKeyDown(Key.D))
                velocity.X = speed;
            else
                velocity.X = 0;

            if(Keyboard.IsKeyDown(Key.Space) && !isJumping)
            {
                velocity.Y = -300;
                isJumping = true;
            }

            Position = new System.Windows.Point(Position.X + velocity.X * deltaTime, Position.Y + velocity.Y * deltaTime);

            //Check if on the ground
            
            var parentWindow = Application.Current.MainWindow;
            if (parentWindow != null && parentWindow.Height == 360)
                groundY = 250;
            else if (parentWindow != null && parentWindow.Height == 720)
                groundY = 600;
            else
                groundY = 800;

            if (groundY == 250 && Position.Y >= 250)
            {
                Position = new System.Windows.Point(Position.X, groundY);
                velocity.Y = 0;
                isJumping = false;
            }
            else if (groundY == 600  && Position.Y >= 600)
            {
                Position = new System.Windows.Point(Position.X, groundY);
                velocity.Y = 0;
                isJumping = false;
            }
            else if(groundY == 800  && Position.Y >= 800)
            {
                Position = new System.Windows.Point(Position.X, groundY);
                velocity.Y = 0;
                isJumping = false;
            }
        }
    }
}
