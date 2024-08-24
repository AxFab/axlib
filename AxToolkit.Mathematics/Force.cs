using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Mathematics;

public struct Force : IEquatable<Force>
{
    public Vector Origin { get; set; }
    public Vector Vector { get; set; }
    public double Value => Vector.Length;

    public static (Vector AccLinear, Vector AccAngular) Resolve(IEnumerable<Force> forces, Vector centerMass, double mass, Vector inertiaMoment)
    {
        const double EPSILON = 0.000001;
        var linear = new Vector();
        var couple = new Vector();
        foreach (var force in forces)
        {
            var thrust = force.Vector.Length;
            if (thrust < EPSILON)
                continue;
            var displacement = force.Origin - centerMass;
            var scalar = Vector.DotProduct(displacement, force.Vector);
            if (displacement.Length < EPSILON)
            {
                linear += force.Vector;
                continue;
            }

            var cos = Math.Max(-1, Math.Min(1, scalar / displacement.Length / thrust));
            if (cos < (EPSILON - 1) || cos > (1 - EPSILON))
            {
                linear += force.Vector;
                continue;
            }

            var angle = -Math.Acos(cos);
            var ss = Math.Sin(angle) * thrust;
            var cs = cos * thrust;

            linear += displacement.Norm * cs;

            // Rotation couple
            var lg = displacement.Length;
            var axis = Vector.CrossProduct(displacement, force.Vector).Norm;
            couple += axis * ss * lg;
        }

        return (linear / mass, couple.DivNum(inertiaMoment));
    }

    public bool Equals(Force other)
    {
        return Origin == other.Origin && Vector == other.Origin;
    }
}
