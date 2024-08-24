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
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AxToolkit.Mathematics;

[JsonConverter(typeof(VectorJsonConverter))]
public struct Vector : IEquatable<Vector>
{
    public Vector() : this(0, 0, 0) { }
    public Vector(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    public Vector(Quaternion a)
    {
        X = a.X;
        Y = a.Y;
        Z = a.Z;
    }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    [JsonIgnore]
    public Vector Norm => this * (1 / Length);
    [JsonIgnore]
    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);
    [JsonIgnore]
    public double LengthSq => X * X + Y * Y + Z * Z;

    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);

    public static double DotProduct(Vector a, Vector b)
        => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector CrossProduct(Vector a, Vector b)
        => new Vector(a.Y * b.Z - b.Y * a.Z, a.Z * b.X - b.Z * a.X, a.X * b.Y - b.X * a.Y);

    public Vector Add(Vector o) => new Vector(X + o.X, Y + o.Y, Z + o.Z);
    public Vector Sub(Vector o) => new Vector(X - o.X, Y - o.Y, Z - o.Z);
    public Vector Mul(double k) => new Vector(X * k, Y * k, Z * k);
    public Vector Div(double k) => new Vector(X / k, Y / k, Z / k);
    public int CompareTo(double k) => LengthSq.CompareTo(k * k);


    public static Vector operator +(Vector a, Vector b) => a.Add(b);

    public static Vector operator -(Vector a, Vector b) => a.Sub(b);

    public static Vector operator *(Vector a, double k) => a.Mul(k);

    public static Vector operator *(double k, Vector a) => a.Mul(k);

    public static Vector operator /(Vector a, double k) => a.Div(k);

    public static Vector operator -(Vector a) => new Vector(-a.X, -a.Y, -a.Z);

    public static bool operator ==(Vector a, Vector b) => a.Equals(b);
    public static bool operator !=(Vector a, Vector b) => !a.Equals(b);

    public bool Equals(Vector o)
        => AxMath.AlmostEqual(X, o.X) && AxMath.AlmostEqual(Y, o.Y) && AxMath.AlmostEqual(Z, o.Z);
 
    public override bool Equals(object obj)
    {
        if (!(obj is Vector v))
            return false;
        return Equals(v);
    }

    public override int GetHashCode() => (int)(44646647 * X + 92508121 * Y + 33184589 * Z);

    //public static Vector operator *(Vector a, Quaternion q)
    //    => throw new NotImplementedException();

    public static double AngleRad(Vector a, Vector b)
        => Math.Acos(DotProduct(a, b) / (a.Length * b.Length));


    public Vector RotateX(double angle)
    {
        var c = Math.Cos(angle);
        var s = Math.Sin(angle);
        return new Vector(X, c * Y - s * Z, s * Y + c * Z);
    }
    public Vector RotateY(double angle)
    {
        var c = Math.Cos(angle);
        var s = Math.Sin(angle);
        return new Vector(c * X + s * Z, Y, -s * X + c * Z);
    }
    public Vector RotateZ(double angle)
    {
        var c = Math.Cos(angle);
        var s = Math.Sin(angle);
        return new Vector(c * X - s * Y, s * X + c * Y, Z);
    }

    private const string FormatString = "0.000";
    public override string ToString() => $"[X:{X.ToString(FormatString, CultureInfo.InvariantCulture)}, Y:{Y.ToString(FormatString, CultureInfo.InvariantCulture)}, Z:{Z.ToString(FormatString, CultureInfo.InvariantCulture)}]";

    public Vector MulNum(Vector v)
        => new Vector(X * v.X, Y * v.Y, Z * v.Z);
    public Vector DivNum(Vector v)
        => new Vector(X / v.X, Y / v.Y, Z / v.Z);

}

public static class VectorEnumerable
{
    public static Vector Sum(this IEnumerable<Vector> source) => source.Sum(x => x);
    public static Vector Sum<T>(this IEnumerable<T> source, Func<T, Vector> selector)
    {
        Vector res = new Vector();
        foreach (var x in source)
            res += selector(x);
        return res;
    }
}

public class VectorJsonConverter : JsonConverter<Vector>
{
    public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
            return new Vector();
        var tokens = str.Split(',');
        return new Vector {
            X = double.Parse(tokens[0], CultureInfo.InvariantCulture),
            Y = double.Parse(tokens[1], CultureInfo.InvariantCulture),
            Z = double.Parse(tokens[2], CultureInfo.InvariantCulture),
        };
    }

    public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options)
    {
        var str = string.Join(',',
            value.X.ToString(CultureInfo.InvariantCulture),
            value.Y.ToString(CultureInfo.InvariantCulture),
            value.Z.ToString(CultureInfo.InvariantCulture)
            );
        writer.WriteStringValue(str);
    }
}
