// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
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
