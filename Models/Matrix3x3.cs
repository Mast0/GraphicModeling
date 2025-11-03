using System.Windows;

namespace GraphicModelling.Models;

public struct Matrix3x3
{
    public double Xx, Xy, Wx, Yx, Yy, Wy, Ox, Oy, wO;

    // m00, m01, m02 -> перший стовпчик матриці (для X)
    // m10, m11, m12 -> другий стовпчик матриці (для Y)
    // m20, m21, m22 -> третій стовпчик матриці (для зсуву і ваги)
    public Matrix3x3(
        double m00, double m01, double m02,
        double m10, double m11, double m12,
        double m20, double m21, double m22)
    {
        Xx = m00; Xy = m01; Wx = m02;
        Yx = m10; Yy = m11; Wy = m12;
        Ox = m20; Oy = m21; wO = m22;
    }

    public static Matrix3x3 Identity => new Matrix3x3(1, 0, 0,
                                                      0, 1, 0,
                                                      0, 0, 1);

    public static Matrix3x3 Multiply(Matrix3x3 a, Matrix3x3 b)
    {
        Matrix3x3 res = new Matrix3x3();

        res.Xx = a.Xx * b.Xx + a.Xy * b.Yx + a.Wx * b.Ox;
        res.Xy = a.Xx * b.Xy + a.Xy * b.Yy + a.Wx * b.Oy;
        res.Wx = a.Xx * b.Wx + a.Xy * b.Wy + a.Wx * b.wO;

        res.Yx = a.Yx * b.Xx + a.Yy * b.Yx + a.Wy * b.Ox;
        res.Yy = a.Yx * b.Xy + a.Yy * b.Yy + a.Wy * b.Oy;
        res.Wy = a.Yx * b.Wx + a.Yy * b.Wy + a.Wy * b.wO;

        res.Ox = a.Ox * b.Xx + a.Oy * b.Yx + a.wO * b.Ox;
        res.Oy = a.Ox * b.Xy + a.Oy * b.Yy + a.wO * b.Oy;
        res.wO = a.Ox * b.Wx + a.Oy * b.Wy + a.wO * b.wO;

        return res;
    }

    public static Matrix3x3 CreateRotation(double angleDegrees, Point center)
    {
        double angleRad = angleDegrees * Math.PI / 180.0;
        double cos = Math.Cos(angleRad);
        double sin = Math.Sin(angleRad);
        double px = center.X;
        double py = center.Y;

        double m20 = -px * (cos - 1) + py * sin;
        double m21 = -px * sin - py * (cos - 1);

        return new Matrix3x3(
            cos, sin, 0,
            -sin, cos, 0,
            m20, m21, 1);
    }

    public static Matrix3x3 CreateScaling(double scale, Point center)
    {
        double px = center.X;
        double py = center.Y;

        double Ox = px * (1 - scale);
        double Oy = py * (1 - scale);

        return new Matrix3x3(
            scale, 0, 0,
            0, scale, 0,
            Ox, Oy, 1);
    }
}
