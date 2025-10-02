using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underground_Psychosis.GameEngine
{
    public abstract class Entity
    {
        public Point Position { get; set; }
        public UIElement Sprite { get; set; }
        public Rect BoundingRect { get; protected set; }
        public abstract void Update(double deltaTime);
        public virtual void Draw(Canvas canvas)
        {
            Canvas.SetLeft(Sprite, Position.X);
            Canvas.SetTop(Sprite, Position.Y);
            if (!canvas.Children.Contains(Sprite))
                canvas.Children.Add(Sprite);
        }
    }
}
