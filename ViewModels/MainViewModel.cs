using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GraphicModelling.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public Lab1ViewModel Lab1ViewModel { get; set; }
    public Lab2ViewModel Lab2ViewModel { get; set; }

    public MainViewModel()
    {
        Lab1ViewModel = new Lab1ViewModel();
        Lab2ViewModel = new Lab2ViewModel();
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion
}
