using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicModelling.Models;

public class Polyhedron
{
    public List<Point3D> Vertices { get; set; } = new List<Point3D>();
    public List<Edge3D> Edges { get; set; } = new List<Edge3D>();

    public static Polyhedron CreateStarPrism(double outerRadius, double innerRadius, double height, int points = 5)
    {
        var mesh = new Polyhedron();
        double angleStep = 360.0 / (points * 2);
        double halfHeight = height / 2.0;

        // Генеруємо дві основи (нижня і верхня)
        for (int layer = 0; layer < 2; layer++)
        {
            double z = layer == 0 ? -halfHeight : halfHeight;
            for (int i = 0; i < points * 2; i++)
            {
                double r = (i % 2 == 0) ? outerRadius : innerRadius;
                double angle = i * angleStep * Math.PI / 180.0;

                angle -= Math.PI / 2.0;

                mesh.Vertices.Add(new Point3D(r * Math.Cos(angle), r * Math.Sin(angle), z));
            }
        }

        int verticesPerLayer = points * 2;

        // З'єднання точок в основах та між ними
        for (int i = 0; i < verticesPerLayer; i++)
        {
            int next = (i + 1) % verticesPerLayer;

            // Ребра нижньої основи
            mesh.Edges.Add(new Edge3D(i, next));

            // Ребра верхньої основи
            mesh.Edges.Add(new Edge3D(i + verticesPerLayer, next + verticesPerLayer));

            // Вертикальні ребра (стіни)
            mesh.Edges.Add(new Edge3D(i, i + verticesPerLayer));
        }

        return mesh;
    }
}
