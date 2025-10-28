using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Underground_Psychosis.GameEngine;

namespace Underground_Psychosis
{
    public partial class InventoryControl : UserControl
    {
        public ObservableCollection<InventorySlot> Slots { get; } = new ObservableCollection<InventorySlot>();

        // follow-related fields
        private Entity? _followTarget;
        private Canvas? _hostCanvas;
        private double _verticalOffset = 8.0;
        private double _smoothFactor = 12.0; // higher = snappier
        private DateTime _lastFrame = DateTime.MinValue;
        private bool _isFollowing = false;

        public InventoryControl()
        {
            InitializeComponent();
            DataContext = this;

            // initialize 6 empty slots (3x2)
            for (int i = 0; i < 6; i++)
                Slots.Add(new InventorySlot());
        }

        // Adds an item to the first empty slot. Does nothing if full.
        public void AddItem(string name, ImageSource icon)
        {
            foreach (var slot in Slots)
            {
                if (slot.Item == null)
                {
                    slot.Item = new InventoryItem { Name = name, Icon = icon };
                    return;
                }
            }
        }

        // Public API: follow an Entity on a Canvas. smoothFactor <=0 disables smoothing.
        public void Follow(Entity target, Canvas hostCanvas, double verticalOffset = 8.0, double smoothFactor = 12.0)
        {
            if (target == null || hostCanvas == null) return;

            // ensure control is added to canvas
            if (!hostCanvas.Children.Contains(this))
                hostCanvas.Children.Add(this);

            _followTarget = target;
            _hostCanvas = hostCanvas;
            _verticalOffset = verticalOffset;
            _smoothFactor = smoothFactor;
            _lastFrame = DateTime.Now;
            if (!_isFollowing)
            {
                CompositionTarget.Rendering += OnRendering;
                _isFollowing = true;
            }

            // set initial immediate position
            UpdatePosition(immediate: true);
        }

        public void Unfollow()
        {
            if (_isFollowing)
            {
                CompositionTarget.Rendering -= OnRendering;
                _isFollowing = false;
            }
            _followTarget = null;
            _hostCanvas = null;
        }

        // Toggle visibility helper
        public void Toggle()
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        // Register a key on a Window to toggle this inventory (convenience)
        public void RegisterToggleKey(Window window, Key key)
        {
            if (window == null) return;
            window.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == key)
                {
                    Toggle();
                    e.Handled = true;
                }
            };
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            UpdatePosition();
        }

        private void UpdatePosition(bool immediate = false)
        {
            if (_followTarget == null || _hostCanvas == null) return;
            if (Visibility != Visibility.Visible) return;

            // determine desired left/top based on entity position + sizes
            double targetCenterX;
            double targetWidth;
            try
            {
                // use dynamic to read Width property exposed by Player classes
                targetCenterX = _followTarget.Position.X + ((_followTarget as dynamic).Width / 2.0);
                targetWidth = ((_followTarget as dynamic).Width);
            }
            catch
            {
                targetCenterX = _followTarget.Position.X;
                targetWidth = 0;
            }

            double inventoryWidth = (ActualWidth > 0 ? ActualWidth : Width);
            double inventoryHeight = (ActualHeight > 0 ? ActualHeight : Height);

            double desiredLeft = targetCenterX - (inventoryWidth / 2.0);
            double desiredTop = _followTarget.Position.Y - inventoryHeight - _verticalOffset;

            // fallback when NaN
            if (double.IsNaN(desiredLeft) || double.IsInfinity(desiredLeft))
                desiredLeft = _followTarget.Position.X - (inventoryWidth / 2.0);
            if (double.IsNaN(desiredTop) || double.IsInfinity(desiredTop))
                desiredTop = _followTarget.Position.Y - inventoryHeight - _verticalOffset;

            // clamp to host canvas bounds so inventory won't be placed off-screen
            double canvasWidth = _hostCanvas.ActualWidth;
            double canvasHeight = _hostCanvas.ActualHeight;

            if (!double.IsNaN(canvasWidth) && !double.IsInfinity(canvasWidth) && canvasWidth > 0)
            {
                desiredLeft = Math.Max(0, Math.Min(desiredLeft, canvasWidth - inventoryWidth));
            }

            if (!double.IsNaN(canvasHeight) && !double.IsInfinity(canvasHeight) && canvasHeight > 0)
            {
                desiredTop = Math.Max(0, Math.Min(desiredTop, canvasHeight - inventoryHeight));
            }

            double currentLeft = Canvas.GetLeft(this);
            double currentTop = Canvas.GetTop(this);

            if (double.IsNaN(currentLeft)) currentLeft = desiredLeft;
            if (double.IsNaN(currentTop)) currentTop = desiredTop;

            if (immediate || _smoothFactor <= 0)
            {
                Canvas.SetLeft(this, desiredLeft);
                Canvas.SetTop(this, desiredTop);
                return;
            }

            // compute deltaTime
            var now = DateTime.Now;
            var dt = (now - _lastFrame).TotalSeconds;
            _lastFrame = now;
            if (dt <= 0) dt = 1.0 / 60.0;

            // lerp towards desired position
            double t = Math.Min(1.0, _smoothFactor * dt);
            double newLeft = currentLeft + (desiredLeft - currentLeft) * t;
            double newTop = currentTop + (desiredTop - currentTop) * t;

            Canvas.SetLeft(this, newLeft);
            Canvas.SetTop(this, newTop);
        }
    }

    public class InventorySlot : INotifyPropertyChanged
    {
        private InventoryItem? _item;
        public InventoryItem? Item
        {
            get => _item;
            set { _item = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class InventoryItem
    {
        public string? Name { get; set; }
        public ImageSource? Icon { get; set; }
    }
}