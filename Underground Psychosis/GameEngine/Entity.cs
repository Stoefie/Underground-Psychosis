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
        public UIElement? Sprite { get; set; }
        public Rect BoundingRect { get; set; }
        public abstract void Update(double deltaTime);
        public virtual void Draw(Canvas canvas)
        {
            if (Sprite == null) return;
            if (!canvas.Children.Contains(Sprite))
                canvas.Children.Add(Sprite);
            Canvas.SetLeft(Sprite, Position.X);
            Canvas.SetTop(Sprite, Position.Y);
        }
    }
}
