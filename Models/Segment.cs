using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GraphicModelling.Models;

public class Segment
{
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
    public double Radius { get; set; }
    public bool IsClockwise { get; set; }

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
    public Segment() { }

    public override string ToString()
    {
        return $"[{StartPoint.X:F1};{StartPoint.Y:F1}] -> [{EndPoint.X:F1};{EndPoint.Y:F1}]";
    }
}
