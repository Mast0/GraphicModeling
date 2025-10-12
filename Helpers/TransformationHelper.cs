using GraphicModelling.Models;
using System;
using System.Windows;

namespace GraphicModelling.Helpers;

public static class TransformationHelper
{
    public static Point ApplyTransformations(
        Point point, Matrix3x3 matrix)
    {
        double x = point.X;
        double y = point.Y;

        double w = x * matrix.M02 + y * matrix.M12 + matrix.M22;

        if (Math.Abs(w) < 1e-9) w = 1e-9;

        double newX = (x * matrix.M00 + y * matrix.M10 + matrix.M20) / w;
        double newY = (x * matrix.M01 + y * matrix.M11 + matrix.M21) / w;

        return new Point(newX, newY);
    }
}
