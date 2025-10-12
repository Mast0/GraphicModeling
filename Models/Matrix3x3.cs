using System.Windows;

namespace GraphicModelling.Models;

public struct Matrix3x3
{
    public double M00, M01, M02, M10, M11, M12, M20, M21, M22;

    // m00, m01, m02 -> перший стовпчик матриці (для X)
    // m10, m11, m12 -> другий стовпчик матриці (для Y)
    // m20, m21, m22 -> третій стовпчик матриці (для зсуву і ваги)
    public Matrix3x3(
        double m00, double m01, double m02,
        double m10, double m11, double m12,
        double m20, double m21, double m22)
    {
        M00 = m00; M01 = m01; M02 = m02;
        M10 = m10; M11 = m11; M12 = m12;
        M20 = m20; M21 = m21; M22 = m22;
    }

    public static Matrix3x3 Identity => new Matrix3x3(1, 0, 0,
                                                      0, 1, 0,
                                                      0, 0, 1);

    public static Matrix3x3 Multiply(Matrix3x3 a, Matrix3x3 b)
    {
        Matrix3x3 res = new Matrix3x3();

        res.M00 = a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20;
        res.M01 = a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21;
        res.M02 = a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22;
        res.M10 = a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20;
        res.M11 = a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21;
        res.M12 = a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22;
        res.M20 = a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20;
        res.M21 = a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21;
        res.M22 = a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22;

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
}
