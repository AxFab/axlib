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
