using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Underground_Psychosis
{
    public partial class InventoryControl : UserControl
    {
        public ObservableCollection<InventorySlot> Slots { get; } = new ObservableCollection<InventorySlot>();

        // raised when a non-empty slot is clicked
        public event Action<InventoryItem>? ItemUsed;

        private Func<Point?>? _followTargetProvider;
        private double _followOffsetY = -80;
        private bool _isFollowing;

        public InventoryControl()
        {
            InitializeComponent();
            DataContext = this;

            // initialize 6 empty slots (3x2)
            for (int i = 0; i < 6; i++)
                Slots.Add(new InventorySlot());

            Unloaded += (s, e) => StopFollow();
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

        // Click handler wired from XAML
        private void Slot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.DataContext is not InventorySlot slot) return;
            var item = slot.Item;
            if (item == null) return;

            ItemUsed?.Invoke(item);
        }

        // Start following a dynamic point (return value should be in window/client coordinates)
        // Example: inventory.Follow(() => gameCanvas.TranslatePoint(new Point(playerCenterX, playerCenterY), Application.Current.MainWindow), offsetY: -100);
        public void Follow(Func<Point?> targetProvider, double offsetY = -80)
        {
            _followTargetProvider = targetProvider ?? throw new ArgumentNullException(nameof(targetProvider));
            _followOffsetY = offsetY;
            if (!_isFollowing)
            {
                CompositionTarget.Rendering += OnRendering;
                _isFollowing = true;
            }
        }

        public void StopFollow()
        {
            if (_isFollowing)
            {
                CompositionTarget.Rendering -= OnRendering;
                _isFollowing = false;
            }
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (_followTargetProvider == null) return;
            var p = _followTargetProvider();
            if (p == null) return;

            // scale values from XAML
            var scale = (PART_Scale?.ScaleX) ?? 1.0;

            // desired top-left so control is centered horizontally above the point with offset
            double desiredLeft = p.Value.X - (ActualWidth * scale) / 2.0;
            double desiredTop = p.Value.Y + _followOffsetY - (ActualHeight * scale) / 2.0;

            // apply to translate transform (it offsets the element from its layout position)
            PART_Translate.X = desiredLeft;
            PART_Translate.Y = desiredTop;
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
        private void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class InventoryItem
    {
        public string? Name { get; set; }
        public ImageSource? Icon { get; set; }
    }
}