using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GraphicModelling.Models;

public class Segment : INotifyPropertyChanged
{
    private Point _startPoint;
    private Point _endPoint;
    private double _radius;

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
    public double Radius 
    { 
        get => _radius;
        set { _radius = value; OnPropertyChanged(); }
    }
    public bool IsClockwise { get; set; }
    public double Length
    {
        get
        {
            return (EndPoint - StartPoint).Length;
        }
        set
        {
            if (Radius > 0) return;

            Vector v = EndPoint - StartPoint;
            double currentLength = v.Length;

            if (currentLength == 0) return;

            // Знаходимо одиночний вектор у напрямку сегмента
            v.Normalize();

            // Обчислюємо новук кінцеву точку рухаючись від початкової
            Point newEndPoint = StartPoint + v * value;

            EndPoint = newEndPoint;
        }
    }

    public Segment(Point start, Point end)
    {
        StartPoint = start;
        EndPoint = end;
        Radius = 0;
        IsClockwise = false;
    }

    public Segment(double startX, double startY, double endX, double endY)
    {
        StartPoint = new Point(startX * 35, startY * 35);
        EndPoint = new Point(endX * 35, endY * 35);
        Radius = 0;
        IsClockwise = false;
    }

    public Segment(double startX, double startY, double endX, double endY, double radius, bool isClockwise = false)
    {
        StartPoint = new Point(startX * 35, startY * 35);
        EndPoint = new Point(endX * 35, endY * 35);
        Radius = radius;
        IsClockwise = isClockwise;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
