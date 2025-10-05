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
                case 2: //Stone
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
                case 3: //Wood
                    {
                        var img = new Image
                        {
                            Width = width,
                            Height = height,
                            Stretch = Stretch.Fill,
                            Source = new BitmapImage(new Uri("pack://application:,,,/images/wood.png"))
                        };
                        return img;
                    }
                case 4: //Log
                    {
                        var img = new Image
                        {
                            Width = width,
                            Height = height,
                            Stretch = Stretch.Fill,
                            Source = new BitmapImage(new Uri("pack://application:,,,/images/log.png"))
                        };
                        return img;
                    }
                case 9: //Purple border
                    {
                        return new Rectangle
                        {
                            Width = width,
                            Height = height,
                            Fill = Brushes.Purple
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
