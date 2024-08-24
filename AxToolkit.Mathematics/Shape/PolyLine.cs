using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Mathematics.Shape;

public class PolyLine
{
    public List<double[]> Points { get; } = new List<double[]>();
    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public int Tag { get; set; }
}
