using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GraphicModelling.Models;

public class CircleShape : INotifyPropertyChanged
{
    private Point _center;
    private double _radius;

    public Point Center 
    { 
        get => _center;
        set { _center = value; OnPropertyChanged(); }
    }
    public double Radius 
    { 
        get => _radius;
        set { _radius = value; OnPropertyChanged(); }
    }

    public CircleShape(double pointX, double pointY, double radius)
    {
        Center = new Point(pointX * 35, pointY * 35);
        Radius = radius * 35;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
