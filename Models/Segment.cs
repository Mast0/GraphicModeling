using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GraphicModelling.Models;

public class Segment
{
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }

    public Segment(Point start, Point end)
    {
        StartPoint = start;
        EndPoint = end;
    }

    public override string ToString()
    {
        return $"[{StartPoint.X:F1};{StartPoint.Y:F1}] -> [{EndPoint.X:F1};{EndPoint.Y:F1}]";
    }
}
