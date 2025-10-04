using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Underground_Psychosis.GameEngine
{
    public static class TileSpriteProvider
    {
        public static UIElement Create(int kind, double width, double height)
        {
            switch (kind)
            {
                case 1: // Solid green block
                {
                    return new Rectangle
                    {
                        Width = width,
                        Height = height,
                        Fill = Brushes.Green
                    };
                }
                case 2: //Image based tile
                {
                    var img = new Image
                    {
                        Width = width,
                        Height = height,
                        Stretch = Stretch.Fill,
                        Source = new BitmapImage(new Uri("pack://application:,,,/images/stone.png"))
                    };
                    return img;
                }
                case 3: //Red
                {
                    return new Rectangle
                    {
                        Width = width,
                        Height = height,
                        Fill = Brushes.Red
                    };
                }
                case 4: //Blue
                {
                    return new Rectangle
                    {
                        Width = width,
                        Height = height,
                        Fill = Brushes.Blue
                    };
                }
                default: // Fallback
                {
                    return new Rectangle
                    {
                        Width = width,
                        Height = height,
                        Fill = Brushes.Gray
                    };
                }
            }
        }
    }
}
