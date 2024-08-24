﻿using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AxToolkit.Mathematics;

[JsonConverter(typeof(VectorJsonConverter))]
public struct Vector
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

    public static Vector operator +(Vector a, Vector b)
        => new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector operator -(Vector a, Vector b)
        => new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector operator *(Vector a, double k)
        => new Vector(a.X * k, a.Y * k, a.Z * k);

    public static Vector operator *(double k, Vector a)
        => new Vector(a.X * k, a.Y * k, a.Z * k);

    public static Vector operator /(Vector a, double k)
        => new Vector(a.X / k, a.Y / k, a.Z / k);

    public static Vector operator /(double k, Vector a)
        => new Vector(k / a.X, k / a.Y, k / a.Z);

    public static Vector operator -(Vector a)
        => new Vector(-a.X, -a.Y, -a.Z);

    public static bool operator ==(Vector a, Vector b)
        => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    public static bool operator !=(Vector a, Vector b)
        => a.X != b.X || a.Y != b.Y || a.Z != b.Z;

    public bool AreEquals(Vector v) => AreEquals(v, 0.00001);
    public bool AreEquals(Vector v, double epsilon)
    {
        var d = this - v;
        return Math.Abs(d.X) < epsilon && Math.Abs(d.Y) < epsilon && Math.Abs(d.Z) < epsilon;
    }
    public override bool Equals(object obj)
    {
        if (!(obj is Vector v))
            return false;
        return this == v;
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