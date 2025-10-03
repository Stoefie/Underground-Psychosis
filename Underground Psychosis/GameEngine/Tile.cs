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

        public Tile(int x, int y, double width, double height, bool isSolid, int kind, Brush fill)
        {
            Width = width;
            Height = height;
            IsSolid = isSolid;
            Kind = kind;
            
            Sprite = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = fill
            };


        Position = new Point(x * width, y * height);
        BoundingRect = new Rect(Position.X, Position.Y, width, height);

        }
        public override void Update(double deltaTime)
        {
            // Tiles are static, so nothing to update
        }
    }
}
