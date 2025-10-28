using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace Underground_Psychosis
{
    public partial class InventoryControl : UserControl
    {
        public ObservableCollection<InventorySlot> Slots { get; } = new ObservableCollection<InventorySlot>();

        public InventoryControl()
        {
            InitializeComponent();
            DataContext = this;

            // initialize 9 empty slots (3x3)
            for (int i = 0; i < 9; i++)
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