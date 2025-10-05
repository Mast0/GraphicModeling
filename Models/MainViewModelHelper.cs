using GraphicModelling.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace GraphicModelling.Models;

public static class MainViewModelHelper
{
    public static ObservableCollection<LineSegment> LineSegments { get; set; }

    public static void AddSampleSquare()
    {
        LineSegments.Add(new LineSegment { StartPoint = new Point(50, 50), EndPoint = new Point(150, 50) });
        LineSegments.Add(new LineSegment { StartPoint = new Point(150, 50), EndPoint = new Point(150, 150) });
        LineSegments.Add(new LineSegment { StartPoint = new Point(150, 150), EndPoint = new Point(50, 150) });
        LineSegments.Add(new LineSegment { StartPoint = new Point(50, 150), EndPoint = new Point(50, 50) });
    }

    public static void AddLine()
    {
        LineSegments.Add(new LineSegment
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(100, 100)
        });
    }

    public static void ClearAll()
    {
        LineSegments.Clear();
    }

    public static void ApplyTranslation(double translateX, double translateY)
    {
        foreach (var s in LineSegments)
        {
            s.StartPoint = new Point(
                s.StartPoint.X + translateX,
                s.StartPoint.Y + translateY);
            s.EndPoint = new Point(
                s.EndPoint.X + translateX,
                s.EndPoint.Y + translateY);
        }
    }

    public static void ApplyScaling(double scaleX, double scaleY)
    {
        var centerX = LineSegments.SelectMany(s => new[] { s.StartPoint.X, s.EndPoint.X }).Average();
        var centerY = LineSegments.SelectMany(s => new[] { s.StartPoint.Y, s.EndPoint.Y }).Average();

        foreach (var s in LineSegments)
        {
            s.StartPoint = new Point(
                centerX + (s.StartPoint.X - centerX) * scaleX,
                centerY + (s.StartPoint.Y - centerY) * scaleY);
            s.EndPoint = new Point(
                centerX + (s.EndPoint.X - centerX) * scaleX,
                centerY + (s.EndPoint.Y - centerY) * scaleY);
        }
    }

    public static void ApplyRotation(double rotateAngle)
    {
        var centerX = LineSegments.SelectMany(s => new[] { s.StartPoint.X, s.EndPoint.X }).Average();
        var centerY = LineSegments.SelectMany(s => new[] { s.StartPoint.Y, s.EndPoint.Y }).Average();
        var angel = rotateAngle * Math.PI / 180.0;
        var cos = Math.Cos(angel);
        var sin = Math.Sin(angel);

        foreach (var s in LineSegments)
        {
            var startReleativeX = s.StartPoint.X - centerX;
            var startReleativeY = s.StartPoint.Y - centerY;
            var endReleativeX = s.EndPoint.X - centerX;
            var endReleativeY = s.EndPoint.Y - centerY;

            s.StartPoint = new Point(
                centerX + startReleativeX * cos - startReleativeY * sin,
                centerY + startReleativeX * sin + startReleativeY * cos);
            s.EndPoint = new Point(
                centerX + endReleativeX * cos - endReleativeY * sin,
                centerY + endReleativeX * sin + endReleativeY * cos);
        }
    }

    public static void ApplyShear(double shearX, double shearY)
    {
        foreach (var s in LineSegments)
        {
            s.StartPoint = new Point(
                s.StartPoint.X + shearX * s.StartPoint.Y,
                s.StartPoint.Y + shearY * s.StartPoint.X);
            s.EndPoint = new Point(
                s.EndPoint.X + shearX * s.EndPoint.Y,
                s.EndPoint.Y + shearY * s.EndPoint.X);
        }
    }

    public static void ApplyProjection(double projectFactor)
    {
        foreach (var s in LineSegments)
        {
            var startZ = 100 + projectFactor;
            var endZ = 100 + projectFactor;

            if (startZ != 0)
            {
                s.StartPoint = new Point(
                    s.StartPoint.X * 100 / startZ,
                    s.StartPoint.Y * 100 / startZ);
            }

            if (endZ != 0)
            {
                s.EndPoint = new Point(
                    s.EndPoint.X * 100 / endZ,
                    s.EndPoint.Y * 100 / endZ);
            }
        }
    }
}
