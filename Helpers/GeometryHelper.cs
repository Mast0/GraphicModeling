using GraphicModelling.Models;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;

namespace GraphicModelling.Helpers;

public static class GeometryHelper
{
    /// <summary>
    /// Розбиває дугу на список точок, що апроксимують її.
    /// </summary>
    /// <param name="arcSegment"></param>
    /// <returns></returns>
    public static List<Point> TessellateArc(Segment arcSegment, double angleStep = 10.0)
    {
        var points = new List<Point>();
        Point start = arcSegment.StartPoint;
        Point end = arcSegment.EndPoint;
        double radius = arcSegment.Radius;

        // Відстань між початковою та кінцевою точками (довжина хорди)
        double chord = (end - start).Length;

        // Якщо радіус замалий для зєднання точок то малюємо пряму лінію
        if (radius <= chord / 2.0)
        {
            points.Add(start);
            points.Add(end);
            return points;
        }

        // Знаходимо центр кола
        Point midPoint = new Point(start.X + (end.X - start.X) / 2.0, start.Y + (end.Y - start.Y) / 2.0);
        double d = Math.Sqrt(radius * radius - (chord / 2.0) * (chord / 2.0));
        Vector v = end - start;
        Vector prep = arcSegment.IsClockwise ? new Vector(-v.Y, v.X) : new Vector(v.Y, -v.X);
        prep.Normalize();
        Point center = midPoint + d * prep;

        // знаходимо початковий та кінцеві кути
        double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
        double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);

        // Виконуємо теселяцію
        double sweepAngle = endAngle - startAngle;
        if (arcSegment.IsClockwise && sweepAngle > 0) sweepAngle -= 2 * Math.PI;
        else if (!arcSegment.IsClockwise && sweepAngle < 0) sweepAngle += 2 * Math.PI;

        // Розбиваємо дугу із кроком приблизно у 18 градус
        int numSteps = (int)Math.Ceiling(Math.Abs(sweepAngle * 180.0 / Math.PI) / angleStep);
        if (numSteps == 0)
        {
            points.Add(start);
            points.Add(end);
            return points;
        }

        double step = sweepAngle / numSteps;

        for (int i = 0; i <= numSteps; i++)
        {
            double currentAngle = startAngle + i * step;
            double px = center.X + radius * Math.Cos(currentAngle);
            double py = center.Y + radius * Math.Sin(currentAngle);
            points.Add(new Point(px, py));
        }

        return points;
    }

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
