using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicModelling.Models;

public class Point3D
{
    public double X, Y, Z, W;

    public Point3D(double x, double y, double z, double w = 1.0)
    {
        X=x; Y=y; Z=z; W=w;
    }
}
