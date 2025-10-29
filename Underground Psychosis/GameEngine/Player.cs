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
        // Movement base values
        private double gravity = 600;
        private double speed = 300;
        private bool isJumping = false;

        // Jumping state
        public bool IsJumping
        {
            get => isJumping;
            set => isJumping = value;
        }

        // Velocity (Speed for movement)
        private Vector velocity;
        public Vector Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        public double Width => (Sprite as Rectangle)?.Width ?? 32;
        public double Height => (Sprite as Rectangle)?.Height ?? 32;

        // Facing direction for sprite swapping
        private bool _facingRight = true;
        private readonly ScaleTransform _flip = new ScaleTransform(1, 1);

        // Dash parameters 
        private double dashSpeed = 1200;
        private double dashDuration = 0.25;
        private double dashCooldown = 0.40;
        private bool allowVerticalMomentumDuringDash = false;
        private bool cancelDashOnHorizontalCollision = true;

        // Dash state
        private bool _isDashing;
        private double _dashTimeRemaining;
        private double _dashCooldownRemaining;
        private bool _dashUsedThisAirtime;

        public bool IsDashing => _isDashing;

        // Health and equipment
        public int MaxHealth { get; } = 100;
        public int Health { get; private set; }
        public string? EquippedWeapon { get; private set; }

        public Player()
        {
            Health = MaxHealth;

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
                Sprite = new Rectangle { Width = 57, Height = 136, Fill = imageBrushPlayerOne };

            Sprite.RenderTransformOrigin = new Point (0.5, 0.5);
            Sprite.RenderTransform = _flip;

            BoundingRect = new Rect(Position.X, Position.Y, Width, Height);
        }

        public override void Update(double deltaTime)
        {
            // Cooldown for dash
            if (_dashCooldownRemaining > 0)
                _dashCooldownRemaining -= deltaTime;
            if (_isDashing)
            {
                _dashTimeRemaining -= deltaTime;
                if (_dashTimeRemaining <= 0)
                    EndDash();
            }

            // Dash start (Shift while airborne)
            if (!_isDashing && isJumping && !_dashUsedThisAirtime && _dashCooldownRemaining <= 0 && (Keyboard.IsKeyDown(Key.LeftShift)))
            {
                StartDash();
            }

            if (!_isDashing)
            {
                if (Keyboard.IsKeyDown(Key.A))
                    velocity.X = -speed;
                else if(Keyboard.IsKeyDown(Key.D))
                    velocity.X = speed;
                else
                    velocity.X = 0;

            /*
            // Movement for left and right
            if(Keyboard.IsKeyDown(Key.A))
                velocity.X = -speed;
                
            else if(Keyboard.IsKeyDown(Key.D))
                velocity.X = speed;
            else
                velocity.X = 0;
            */


                // Flipping sprite based on direction
                if (velocity.X < 0 && _facingRight)
                {
                    _facingRight = false;
                    _flip.ScaleX = -1;
                }
                else if (velocity.X > 0  && !_facingRight)
                {
                    _facingRight = true;
                    _flip.ScaleX = 1;
                }
            }

            // Jumping movemement
            if(Keyboard.IsKeyDown(Key.Space) && !isJumping)
            {
                velocity.Y = -450;
                isJumping = true;
            }

            // System for gravity
            if (!_isDashing || allowVerticalMomentumDuringDash)
            {
                velocity.Y += gravity * deltaTime;
            }
            else
            {
                velocity.Y = 0;
            }
            // Integrate movement into position checks
            Position = new Point(
                Position.X + velocity.X * deltaTime,
                Position.Y + velocity.Y * deltaTime);

            //Updating the bounds check
            BoundingRect = new Rect(Position.X, Position.Y, Width, Height);
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimeRemaining = dashDuration;
            _dashCooldownRemaining = dashCooldown;
            _dashUsedThisAirtime = true;

            int dir = _facingRight ? 1 : -1;
            if (velocity.X != 0)
                dir = Math.Sign(velocity.X);

            velocity.X = dir * dashSpeed;

            if (!allowVerticalMomentumDuringDash)
                velocity.Y = 0;
        }

        private void EndDash()
        {
            _isDashing = false;
            // Optionally keep some carry-over horizontal speed, or clamp:
            if (Math.Abs(velocity.X) > speed)
                velocity.X = Math.Sign(velocity.X) * speed;
        }

        // Health & equipment API
        public void Heal(int amount)
        {
            if (amount <= 0) return;
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public void Equip(string? weaponName)
        {
            EquippedWeapon = weaponName;
        }

        public void ResolveCollisions(System.Collections.Generic.IEnumerable<Tile> solidTiles)
        {
            foreach (var tile in solidTiles)
            {
                if (!tile.IsSolid)
                    continue;
                
                if (!BoundingRect.IntersectsWith(tile.BoundingRect))
                    continue;

                // Calculate whether it overlaps

                double playerLeft = BoundingRect.Left;
                double playerRight = BoundingRect.Right;
                double playerTop = BoundingRect.Top;
                double playerBottom = BoundingRect.Bottom;

                double tileLeft = tile.BoundingRect.Left;
                double tileRight = tile.BoundingRect.Right;
                double tileTop = tile.BoundingRect.Top;
                double tileBottom = tile.BoundingRect.Bottom;

                double overlapX = Math.Min(playerRight - tileLeft, tileRight - playerLeft);
                double overlapY = Math.Min(playerBottom - tileTop, tileBottom - playerTop);

                // Resolve on the shallow axis
                if (overlapX < overlapY)
                {
                    // Horizontal resolution
                    if (playerRight > tileLeft && playerLeft < tileLeft) // Coming from left
                    {
                        Position = new Point(Position.X - overlapX, Position.Y);
                    }
                    else
                    {
                        Position = new Point(Position.X + overlapX, Position.Y);
                    }
                    velocity.X = 0;

                    if (_isDashing && cancelDashOnHorizontalCollision)
                        EndDash();
                }
                else
                {
                    // Vertical resolution
                    if(playerBottom > tileTop && playerTop < tileTop) // Falling onto tile
                    {
                        Position = new Point(Position.X, Position.Y - overlapY);
                        velocity.Y = 0;
                        isJumping = false;

                        // Reset dash
                        _dashUsedThisAirtime = false;
                        if (_isDashing) EndDash();
                    }
                    else // Hitting head against tile
                    {
                        Position = new Point(Position.X, Position.Y + overlapY);
                        velocity.Y = 0;
                    }
                }

                // Update bounds after adjustment
                BoundingRect = new Rect(Position.X, Position.Y, Width, Height);
            }
        }
    }
}




        /*
            
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
                isJumping = false; */

