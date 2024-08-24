using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Mathematics;

public struct SphericalCoordinate
{
    public SphericalCoordinate(Vector vector)
    {
        Radius = vector.Length;
        Teta = Math.Atan2(vector.Y, vector.X);
        var dz = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        Phi = Math.Atan2(dz, vector.Z);
    }
    public double Radius { get; set; }
    public double Teta { get; set; }
    public double Phi { get; set; }

    public Vector Vector => new Vector(
            Radius * Math.Sin(Phi) * Math.Cos(Teta),
            Radius * Math.Sin(Phi) * Math.Sin(Teta),
            Radius * Math.Cos(Phi));
}
