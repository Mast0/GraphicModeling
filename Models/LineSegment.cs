using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GraphicModelling.Models;

public class LineSegment : INotifyPropertyChanged
{
    private Point _startPoint;
    private Point _endPoint;

    public Point StartPoint
    {
        get => _startPoint;
        set { _startPoint = value; OnPropertyChanged(); }
    }

    public Point EndPoint
    {
        get => _endPoint;
        set { _endPoint = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
