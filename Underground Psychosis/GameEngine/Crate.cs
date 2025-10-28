using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Underground_Psychosis.GameEngine
{
    public class Crate : Entity
    {
        // Use Entity.Position (inherited) instead of a shadowing field
        public Vector Velocity;
        public double Width;
        public double Height;

        private readonly Dictionary<(int x, int y), Tile> _tiles;
        private readonly Func<double> _tileWidth;
        private readonly Func<double> _tileHeight;

        private readonly Player _p1;
        private readonly PlayerTwo _p2;

        private double _gravity = 600.0;
        private double _maxFallSpeed = 2000.0;

        public Crate(
            Point startPosition,
            double width,
            double height,
            Dictionary<(int x, int y), Tile> tiles,
            Func<double> tileWidthProvider,
            Func<double> tileHeightProvider,
            Player p1,
            PlayerTwo p2)
        {
            Position = startPosition;
            Width = width;
            Height = height;

            _tiles = tiles;
            _tileWidth = tileWidthProvider;
            _tileHeight = tileHeightProvider;

            _p1 = p1;
            _p2 = p2;
        }

        private Rect Bounds => new Rect(Position.X, Position.Y, Width, Height);

        public override void Update(double deltaTime)
        {
            // Apply gravity
            Velocity.Y = Math.Min(Velocity.Y + _gravity * deltaTime, _maxFallSpeed);

            // Player interactions (push and stand)
            HandlePlayerInteraction(_p1, deltaTime);
            HandlePlayerInteraction(_p2, deltaTime);

            // Swept AABB per-axis
            double dx = Velocity.X * deltaTime;
            double dy = Velocity.Y * deltaTime;

            if (Math.Abs(dx) > 0.0)
                MoveX(dx);

            if (Math.Abs(dy) > 0.0)
                MoveY(dy);

            // Keep inside level bounds (prevents disappearing off-screen)
            ClampToTilemapBounds();

            BoundingRect = Bounds;
        }

        public override void Draw(Canvas canvas)
        {
            if (Sprite is not Rectangle r)
            {
                r = new Rectangle
                {
                    Width = Width,
                    Height = Height,
                    Fill = new SolidColorBrush(Color.FromRgb(138, 90, 54)),
                    Stroke = Brushes.SaddleBrown,
                    StrokeThickness = Math.Max(1, Math.Min(Width, Height) * 0.05)
                };
                Sprite = r;
                canvas.Children.Add(r);
            }

            Canvas.SetLeft(r, Position.X);
            Canvas.SetTop(r, Position.Y);
        }

        private void HandlePlayerInteraction(object? playerObj, double dt)
        {
            if (playerObj is null) return;

            // Extract common values without reusing the same pattern variable names
            double px, py, pw, ph;
            if (playerObj is Player pOne)
            {
                px = pOne.Position.X; py = pOne.Position.Y; pw = pOne.Width; ph = pOne.Height;
            }
            else if (playerObj is PlayerTwo pTwo)
            {
                px = pTwo.Position.X; py = pTwo.Position.Y; pw = pTwo.Width; ph = pTwo.Height;
            }
            else return;

            var pRect = new Rect(px, py, pw, ph);
            var cRect = Bounds;

            if (!pRect.IntersectsWith(cRect)) return;

            double overlapX = Math.Min(pRect.Right, cRect.Right) - Math.Max(pRect.Left, cRect.Left);
            double overlapY = Math.Min(pRect.Bottom, cRect.Bottom) - Math.Max(pRect.Top, cRect.Top);
            if (overlapX <= 0 || overlapY <= 0) return;

            bool resolveHorizontal = overlapX < overlapY;

            if (resolveHorizontal)
            {
                // Push crate horizontally based on which side the player is
                if (pRect.Left < cRect.Left)
                {
                    Position = new Point(Position.X + overlapX, Position.Y);
                    // Resolve strictly along X
                    ResolveCollisionsAlongAxis(axisX: true);
                }
                else
                {
                    Position = new Point(Position.X - overlapX, Position.Y);
                    // Resolve strictly along X
                    ResolveCollisionsAlongAxis(axisX: true);
                }
            }
            else
            {
                // Vertical separation
                if (pRect.Top < cRect.Top)
                {
                    // Player above crate -> snap on top and ground them
                    double newPlayerY = cRect.Top - ph;

                    if (playerObj is Player pTopOne)
                    {
                        pTopOne.Position = new Point(pTopOne.Position.X, newPlayerY);
                        pTopOne.Velocity = new Vector(pTopOne.Velocity.X, 0);
                        pTopOne.IsJumping = false;
                    }
                    else if (playerObj is PlayerTwo pTopTwo)
                    {
                        pTopTwo.Position = new Point(pTopTwo.Position.X, newPlayerY);
                        pTopTwo.Velocity = new Vector(pTopTwo.Velocity.X, 0);
                        pTopTwo.IsJumping = false;
                    }
                }
                else
                {
                    // Player below (head bump) -> move crate up a bit
                    Position = new Point(Position.X, Position.Y - overlapY);
                    // Resolve strictly along Y
                    ResolveCollisionsAlongAxis(axisX: false);
                }
            }
        }

        private void MoveX(double dx)
        {
            Position = new Point(Position.X + dx, Position.Y);

            // Iterate to settle against multiple tiles if needed
            for (int i = 0; i < 3; i++)
            {
                if (!ResolveCollisionsAlongAxis(axisX: true))
                    break;
            }
        }

        private void MoveY(double dy)
        {
            Position = new Point(Position.X, Position.Y + dy);

            // Iterate to settle against multiple tiles if needed
            for (int i = 0; i < 3; i++)
            {
                if (!ResolveCollisionsAlongAxis(axisX: false))
                    break;
            }
        }

        // Resolves collisions only along the specified axis.
        private bool ResolveCollisionsAlongAxis(bool axisX)
        {
            bool hit = false;

            foreach (var t in GetNearbySolidTiles())
            {
                var tRect = new Rect(t.Position.X, t.Position.Y, t.Width, t.Height);
                var cRect = Bounds;

                if (!cRect.IntersectsWith(tRect)) continue;

                double overlapX = Math.Min(cRect.Right, tRect.Right) - Math.Max(cRect.Left, tRect.Left);
                double overlapY = Math.Min(cRect.Bottom, tRect.Bottom) - Math.Max(cRect.Top, tRect.Top);
                if (overlapX <= 0 || overlapY <= 0) continue;

                hit = true;

                if (axisX)
                {
                    // Decide push direction by comparing centers
                    double cCenterX = cRect.Left + cRect.Width * 0.5;
                    double tCenterX = tRect.Left + tRect.Width * 0.5;

                    if (cCenterX < tCenterX)
                        Position = new Point(Position.X - overlapX, Position.Y);
                    else
                        Position = new Point(Position.X + overlapX, Position.Y);

                    Velocity = new Vector(0, Velocity.Y);
                }
                else
                {
                    // Decide push direction by comparing centers
                    double cCenterY = cRect.Top + cRect.Height * 0.5;
                    double tCenterY = tRect.Top + tRect.Height * 0.5;

                    if (cCenterY < tCenterY)
                        Position = new Point(Position.X, Position.Y - overlapY); // hit ceiling
                    else
                        Position = new Point(Position.X, Position.Y + overlapY); // landed on tile

                    Velocity = new Vector(Velocity.X, 0);
                }
            }

            return hit;
        }

        private IEnumerable<Tile> GetNearbySolidTiles()
        {
            var tw = _tileWidth();
            var th = _tileHeight();
            if (tw <= 0 || th <= 0) yield break;

            int minX = Math.Max(0, (int)Math.Floor(Bounds.Left / tw) - 1);
            int maxX = (int)Math.Ceiling(Bounds.Right / tw) + 1;
            int minY = Math.Max(0, (int)Math.Floor(Bounds.Top / th) - 1);
            int maxY = (int)Math.Ceiling(Bounds.Bottom / th) + 1;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (_tiles.TryGetValue((x, y), out var tile) && tile.IsSolid)
                        yield return tile;
                }
            }
        }

        private void ClampToTilemapBounds()
        {
            var tw = _tileWidth();
            var th = _tileHeight();
            if (_tiles.Count == 0 || tw <= 0 || th <= 0) return;

            // Assuming tiles are laid out from (0,0) to (maxX,maxY)
            int maxGridX = 0, maxGridY = 0;
            foreach (var key in _tiles.Keys)
            {
                if (key.x > maxGridX) maxGridX = key.x;
                if (key.y > maxGridY) maxGridY = key.y;
            }

            double worldW = (maxGridX + 1) * tw;
            double worldH = (maxGridY + 1) * th;

            double clampedX = Math.Clamp(Position.X, 0, Math.Max(0, worldW - Width));
            double clampedY = Math.Clamp(Position.Y, 0, Math.Max(0, worldH - Height));

            if (clampedX != Position.X || clampedY != Position.Y)
                Position = new Point(clampedX, clampedY);
        }
    }
}