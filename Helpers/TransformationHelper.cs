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

        double w = x * matrix.Wx + y * matrix.Wy + matrix.wO;
        double Wx = matrix.Wx == 0 ? 1 : matrix.Wx;
        double Wy = matrix.Wy == 0 ? 1 : matrix.Wy;

        if (Math.Abs(w) < 1e-9) w = 1e-9;

        double newX = (x * matrix.Xx + y * matrix.Yx + matrix.Ox) / w;
        double newY = (x * matrix.Xy + y * matrix.Yy + matrix.Oy) / w;

        return new Point(newX, newY);
    }
}
