using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

// purpose of this class is to keep track of changes for example losing health, picking items up, timer etc

public class GameState : INotifyPropertyChanged
{
    private TimeSpan timer;
    public TimeSpan Timer
    {
        get => timer;
        set { timer = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
