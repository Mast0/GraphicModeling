using GraphicModelling.Models;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;

namespace GraphicModelling.Helpers;

public static class GeometryHelper
{
    /// <summary>
    /// Розбиває коло на список точок, що апроксимують його
    /// </summary>
    /// <param name="circle"></param>
    /// <returns></returns>
    public static List<Point> TessellateCircle(CircleShape circle, double angleStep = 10.0)
    {
        var points = new List<Point>();
        Point center = circle.Center;
        double radius = circle.Radius;

        for (double angle = 0; angle < 360; angle += angleStep)
        {
            // конвертуємо градуси в радіани
            double angleRad = angle * Math.PI / 180.0;

            // Формула знаходження точки на колі
            double px = center.X + radius * Math.Cos(angleRad);
            double py = center.Y + radius * Math.Sin(angleRad);

            points.Add(new Point(px, py));
        }

        // додаємо першу точку в кінець щоб замкнути фігуру
        if (points.Count > 0)
            points.Add(points[0]);

        return points;
    }
}
