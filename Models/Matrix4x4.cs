using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicModelling.Models;

public class Matrix4x4
{
    // Матриця 4x4 зберігається як двовимірний масив або набір полів. 
    // Для зручності використаємо масив [row, col]
    public double[,] M;

    public Matrix4x4()
    {
        M = new double[4, 4];
    }

    public static Matrix4x4 Identity 
    { 
        get
        {
            var m = new Matrix4x4();
            m.M[0, 0] = 1;
            m.M[1, 1] = 1;
            m.M[2, 2] = 1;
            m.M[3, 3] = 1;
            return m;
        }
    }

    public static Matrix4x4 Multiply(Matrix4x4 A, Matrix4x4 B)
    {
        var res = new Matrix4x4();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                res.M[i, j] = 0;
                for (int k = 0; k < 4; k++)
                {
                    res.M[i, j] += A.M[i, k] * B.M[k, j];
                }
            }
        }
        return res;
    }

    public static Point3D Multiply(Point3D p, Matrix4x4 m)
    {
        double x = p.X * m.M[0, 0] + p.Y * m.M[1, 0] + p.Z * m.M[2, 0] + p.W * m.M[3, 0];
        double y = p.X * m.M[0, 1] + p.Y * m.M[1, 1] + p.Z * m.M[2, 1] + p.W * m.M[3, 1];
        double z = p.X * m.M[0, 2] + p.Y * m.M[1, 2] + p.Z * m.M[2, 2] + p.W * m.M[3, 2];
        double w = p.X * m.M[0, 3] + p.Y * m.M[1, 3] + p.Z * m.M[2, 3] + p.W * m.M[3, 3];
        return new Point3D(x, y, z, w);
    }

    public static Matrix4x4 Translation(double dx, double dy, double dz)
    {
        var m = Identity;
        m.M[3, 0] = dx;
        m.M[3, 1] = dy;
        m.M[3, 2] = dz;
        return m;
    }

    public static Matrix4x4 Scaling(double sx, double sy, double sz)
    {
        var m = Identity;
        m.M[0, 0] = sx;
        m.M[1, 1] = sy;
        m.M[2, 2] = sz;
        return m;
    }

    public static Matrix4x4 RotationX(double angleDeg)
    {
        var m = Identity;
        double rad = Math.PI * angleDeg / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        m.M[1, 1] = cos; m.M[1, 2] = sin;
        m.M[2, 1] = -sin; m.M[2, 2] = cos;
        return m;
    }

    public static Matrix4x4 RotationY(double angleDeg)
    {
        var m = Identity;
        double rad = Math.PI * angleDeg / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        m.M[0, 0] = cos; m.M[0, 2] = -sin;
        m.M[2, 0] = sin; m.M[2, 2] = cos;
        return m;
    }

    public static Matrix4x4 RotationZ(double angleDeg)
    {
        var m = Identity;
        double rad = Math.PI * angleDeg / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        m.M[0, 0] = cos; m.M[0, 1] = sin;
        m.M[1, 0] = -sin; m.M[1, 1] = cos;
        return m;
    }
}
