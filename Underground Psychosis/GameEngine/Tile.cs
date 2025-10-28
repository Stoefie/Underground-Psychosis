using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;

namespace Underground_Psychosis.GameEngine
{
    public class Tile : Entity
    {

        public double Width { get; }
        public double Height { get; }
        public bool IsSolid { get; }
        public int Kind { get; }

        public Tile(int x, int y, double width, double height, bool isSolid, int kind, Brush? fill = null)
        {
            Width = width;
            Height = height;
            IsSolid = isSolid;
            Kind = kind;
            
            // If a fill Brush was explicitly passed, prefer it.
            // Otherwise use the sprite provider to supply the correct UIElement for this kind.
            if (fill != null)
            {
                Sprite = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = fill
                };
            }
            else
            {
                Sprite = TileSpriteProvider.Create(kind, width, height);
            }

            Position = new Point(x * width, y * height);
            BoundingRect = new Rect(Position.X, Position.Y, width, height);
        }
        public override void Update(double deltaTime)
        {
            // Tiles are static, so nothing to update
        }
    }
}
