using System.Windows;

namespace GraphicModelling.Models;

public class CircleShape
{
    public Point Center { get; set; }
    public double Radius { get; set; }

    public CircleShape(double pointX, double pointY, double radius)
    {
        Center = new Point(pointX * 35, pointY * 35);
        Radius = radius * 35;
    }
}
