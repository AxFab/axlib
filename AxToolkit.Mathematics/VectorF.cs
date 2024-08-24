using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AxToolkit.Mathematics;

[JsonConverter(typeof(VectorFJsonConverter))]
public struct VectorF
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

    public static VectorF operator +(VectorF a, VectorF b)
        => new VectorF(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static VectorF operator -(VectorF a, VectorF b)
        => new VectorF(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static VectorF operator *(VectorF a, float k)
        => new VectorF(a.X * k, a.Y * k, a.Z * k);

    public static VectorF operator *(float k, VectorF a)
        => new VectorF(a.X * k, a.Y * k, a.Z * k);

    public static VectorF operator /(VectorF a, float k)
        => new VectorF(a.X / k, a.Y / k, a.Z / k);

    public static VectorF operator /(float k, VectorF a)
        => new VectorF(a.X / k, a.Y / k, a.Z / k);

    public static VectorF operator -(VectorF a)
        => new VectorF(-a.X, -a.Y, -a.Z);

    public static bool operator ==(VectorF a, VectorF b)
        => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    public static bool operator !=(VectorF a, VectorF b)
        => a.X != b.X || a.Y != b.Y || a.Z != b.Z;

    public bool AreEquals(VectorF v) => AreEquals(v, 0.0001f);
    public bool AreEquals(VectorF v, float epsilon)
    {
        var d = this - v;
        return Math.Abs(d.X) < epsilon && Math.Abs(d.Y) < epsilon && Math.Abs(d.Z) < epsilon;
    }
    public override bool Equals(object obj)
    {
        if (!(obj is VectorF v))
            return false;
        return this == v;
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