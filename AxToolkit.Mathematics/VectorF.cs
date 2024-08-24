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

[JsonConverter(typeof(VectorFJsonConverter))]
public struct VectorF : IEquatable<VectorF>
{
    public VectorF() : this(0, 0, 0) { }
    public VectorF(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    public VectorF(Quaternion a)
    {
        X = (float)a.X;
        Y = (float)a.Y;
        Z = (float)a.Z;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    [JsonIgnore]
    public VectorF Norm => this * (1 / Length);
    [JsonIgnore]
    public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
    [JsonIgnore]
    public float LengthSq => X * X + Y * Y + Z * Z;

    public static float DotProduct(VectorF a, VectorF b)
        => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static VectorF CrossProduct(VectorF a, VectorF b)
        => new VectorF(a.Y * b.Z - b.Y * a.Z, a.Z * b.X - b.Z * a.X, a.X * b.Y - b.X * a.Y);

    public VectorF Add(VectorF o) => new VectorF(X + o.X, Y + o.Y, Z + o.Z);
    public VectorF Sub(VectorF o) => new VectorF(X - o.X, Y - o.Y, Z - o.Z);
    public VectorF Mul(float k) => new VectorF(X * k, Y * k, Z * k);
    public VectorF Div(float k) => new VectorF(X / k, Y / k, Z / k);
    public int CompareTo(float k) => LengthSq.CompareTo(k * k);


    public static VectorF operator +(VectorF a, VectorF b) => a.Add(b);

    public static VectorF operator -(VectorF a, VectorF b) => a.Sub(b);

    public static VectorF operator *(VectorF a, float k) => a.Mul(k);

    public static VectorF operator *(float k, VectorF a) => a.Mul(k);

    public static VectorF operator /(VectorF a, float k) => a.Div(k);

    public static VectorF operator -(VectorF a) => new VectorF(-a.X, -a.Y, -a.Z);

    public static bool operator ==(VectorF a, VectorF b) => a.Equals(b);
    public static bool operator !=(VectorF a, VectorF b) => !a.Equals(b);

    public bool Equals(VectorF o)
        => AxMath.AlmostEqual(X, o.X) && AxMath.AlmostEqual(Y, o.Y) && AxMath.AlmostEqual(Z, o.Z);


    public override bool Equals(object obj)
    {
        if (!(obj is VectorF v))
            return false;
        return Equals(v);
    }

    public override int GetHashCode() => (int)(44646647 * X + 92508121 * Y + 33184589 * Z);

    public static float AngleRad(VectorF a, VectorF b)
        => (float)Math.Acos(DotProduct(a, b) / (a.Length * b.Length));


    public VectorF RotateX(float angle)
    {
        var c = (float)Math.Cos(angle);
        var s = (float)Math.Sin(angle);
        return new VectorF(X, c * Y - s * Z, s * Y + c * Z);
    }
    public VectorF RotateY(float angle)
    {
        var c = (float)Math.Cos(angle);
        var s = (float)Math.Sin(angle);
        return new VectorF(c * X + s * Z, Y, -s * X + c * Z);
    }
    public VectorF RotateZ(float angle)
    {
        var c = (float)Math.Cos(angle);
        var s = (float)Math.Sin(angle);
        return new VectorF(c * X - s * Y, s * X + c * Y, Z);
    }

    private const string FormatString = "0.000";
    public override string ToString() => $"[X:{X.ToString(FormatString, CultureInfo.InvariantCulture)}, Y:{Y.ToString(FormatString, CultureInfo.InvariantCulture)}, Z:{Z.ToString(FormatString, CultureInfo.InvariantCulture)}]";

    public VectorF MulNum(VectorF v)
        => new VectorF(X * v.X, Y * v.Y, Z * v.Z);
    public VectorF DivNum(VectorF v)
        => new VectorF(X / v.X, Y / v.Y, Z / v.Z);
}

public static class VectorFEnumerable
{
    public static VectorF Sum(this IEnumerable<VectorF> source) => source.Sum(x => x);
    public static VectorF Sum<T>(this IEnumerable<T> source, Func<T, VectorF> selector)
    {
        VectorF res = new VectorF();
        foreach (var x in source)
            res += selector(x);
        return res;
    }
}

public class VectorFJsonConverter : JsonConverter<VectorF>
{
    public override VectorF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
            return new VectorF();
        var tokens = str.Split(',');
        return new VectorF
        {
            X = float.Parse(tokens[0], CultureInfo.InvariantCulture),
            Y = float.Parse(tokens[1], CultureInfo.InvariantCulture),
            Z = float.Parse(tokens[2], CultureInfo.InvariantCulture),
        };
    }

    public override void Write(Utf8JsonWriter writer, VectorF value, JsonSerializerOptions options)
    {
        var str = string.Join(',',
            value.X.ToString(CultureInfo.InvariantCulture),
            value.Y.ToString(CultureInfo.InvariantCulture),
            value.Z.ToString(CultureInfo.InvariantCulture)
            );
        writer.WriteStringValue(str);
    }
}
