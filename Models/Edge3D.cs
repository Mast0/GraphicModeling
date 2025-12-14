using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicModelling.Models;

public class Edge3D
{
    public int StartIndex;
    public int EndIndex;

    public Edge3D(int start, int end)
    {
        StartIndex = start;
        EndIndex = end;
    }
}
