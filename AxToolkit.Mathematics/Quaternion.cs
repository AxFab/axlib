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

[JsonConverter(typeof(QuaternionJsonConverter))]
public struct Quaternion : IEquatable<Quaternion>
{
    public Quaternion() : this(0, 0, 0, 0) { }
    public Quaternion(double x, double y, double z, double w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Quaternion(Vector axis, double angleRad)
    {
        var half = 0.5 * angleRad;
        axis = axis.Norm * Math.Sin(half);
        X = axis.X;
        Y = axis.Y;
        Z = axis.Z;
        W = Math.Cos(half);
        var length = Length;
        X /= length;
        Y /= length;
        Z /= length;
        W /= length;
    }

    public static Quaternion From(Vector u, Vector v)
    {
        var dot = Vector.DotProduct(u, v);
        var s = Math.Sqrt(2 * (1 + dot));
        var q = Vector.CrossProduct(u, v) * 1 / s;
        return new Quaternion(q.X, q.Y, q.Z, 0.5 * s);
    }
    public static Quaternion WithW(Vector v) => WithW(v, 0);
    public static Quaternion WithW(Vector v, double w) => new Quaternion(v.X, v.Y, v.Z, w);

    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
    public double LengthSq => X * X + Y * Y + Z * Z + W * W;

    public bool IsRotation
    {
        get
        {
            var x = Transform(new Vector(1, 0, 0));
            var y = Transform(new Vector(0, 1, 0));
            var z = Transform(new Vector(0, 0, 1));
            return Math.Abs(x.Length - 1) < 0.0000000000001
                && Math.Abs(y.Length - 1) < 0.0000000000001
                && Math.Abs(z.Length - 1) < 0.0000000000001;
        }
    }

    public Quaternion Norm
    {
        get
        {
            var l = Length;
            return new Quaternion(X / l, Y / l, Z / l, W / l);
        }
    }
    public Quaternion Conjugate => new Quaternion(-X, -Y, -Z, W);

    public Vector Transform(Vector vector)
    {
        // Transform v -> q . p . q-1
        var p = WithW(vector);
        var q = this;
        var r = Concat(Concat(q, p), q.Conjugate);
        return r.Vector;
    }
    public static Quaternion FromEulerAngles(Vector v)
        => FromEulerAngles(v.X, v.Y, v.Z);

    public static Quaternion FromEulerAngles(double x, double y, double z) // roll, pitch, yaw
    {
        var cy = Math.Cos(z * 0.5);
        var sy = Math.Sin(z * 0.5);
        var cp = Math.Cos(y * 0.5);
        var sp = Math.Sin(y * 0.5);
        var cr = Math.Cos(x * 0.5);
        var sr = Math.Sin(x * 0.5);

        return new Quaternion(
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy,
            cr * cp * cy + sr * sp * sy
            );
    }

    public Vector TransformRotation(Vector eulerAngles)
    {
        var quaternion = Quaternion.FromEulerAngles(eulerAngles);
        var angle = quaternion.AngleRad;
        if (Math.Abs(angle) < 1e-6)
            return new Vector();
        var axis = quaternion.Axis;
        var transformedAxis = Transform(axis);
        return new Quaternion(transformedAxis, angle).EulerAngles;
    }

    public static double DotProduct(Quaternion p, Quaternion q)
        => p.X * q.X + p.Y * q.Y + p.Z * q.Z + p.W * q.W;

    public Quaternion Add(Quaternion o)
        => new Quaternion(X + o.X, Y + o.Y, Z + o.Z, W + o.W);
    public Quaternion Sub(Quaternion o)
        => new Quaternion(X - o.X, Y - o.Y, Z - o.Z, W - o.W);
    public Quaternion Subtract(Quaternion o) => Sub(o);
    
    public static Quaternion operator +(Quaternion a, Quaternion b) => a.Add(b);
    public static Quaternion operator -(Quaternion a, Quaternion b) => a.Sub(b);

    public static Quaternion Concat(Quaternion q0, Quaternion q1)
        => new Quaternion(
            q0.W * q1.X + q0.X * q1.W + q0.Y * q1.Z - q0.Z * q1.Y,
            q0.W * q1.Y - q0.X * q1.Z + q0.Y * q1.W + q0.Z * q1.X,
            q0.W * q1.Z + q0.X * q1.Y - q0.Y * q1.X + q0.Z * q1.W,
            q0.W * q1.W - q0.X * q1.X - q0.Y * q1.Y - q0.Z * q1.Z
        );

    public static Quaternion CrossProduct(Quaternion a, Quaternion b)
    {
        var p = new Vector(a);
        var q = new Vector(b);
        var r = Vector.CrossProduct(p, q) + (p * b.W) + (q * a.W);
        return WithW(r, a.W * b.W - Vector.DotProduct(p, q));
    }


    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double W { get; set; }

    public Vector Vector => new Vector(X, Y, Z);


    public static bool operator ==(Quaternion a, Quaternion b)
        => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
    public static bool operator !=(Quaternion a, Quaternion b)
        => a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W == b.W;

    public override int GetHashCode() => (int)(44646647 * X + 92508121 * Y + 33184589 * Z + 66905921 * W);

    public Vector Axis
    {
        get
        {
            if (Math.Abs(W) > 1)
                return Norm.Axis; // if w>1 acos and sqrt will produce errors, this cant happen if quaternion is normalised
            double s = Math.Sqrt(1 - W * W); // assuming quaternion normalised then w is less than 1, so term always positive.
            if (s < 1e-9) // avoid divide by zero, if s close to zero, then direction of axis not important
                return new Vector(1, 0, 0);
            else
                return new Vector(X / s, Y / s, Z / s).Norm;
        }
    }

    public double AngleRad => Math.Abs(W) > 1 ? Norm.AngleRad : 2 * Math.Acos(W);

    public Vector EulerAngles
    {
        get {
            // roll (x-axis rotation)
            double sinr_cosp = 2 * (W * X + Y * Z);
            double cosr_cosp = 1 - 2 * (X * X + Y * Y);
            var roll = Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = Math.Sqrt(1 + 2 * (W * Y - X * Z));
            double cosp = Math.Sqrt(1 - 2 * (W * Y - X * Z));
            var pitch = 2 * Math.Atan2(sinp, cosp) - Math.PI / 2;

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (W * Z + X * Y);
            double cosy_cosp = 1 - 2 * (Y * Y + Z * Z);
            var yaw = Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector(roll, pitch, yaw);
        }
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Quaternion v))
            return false;
        return this == v;
    }

    public override string ToString() => FormattableString.Invariant($"[X:{X:0.000}, Y:{Y:0.000}, Z:{Z:0.000}, W:{W:0.000}]");

    public static Quaternion FromToRotation(Vector from, Vector to)
    {
        from = from.Norm;
        to = to.Norm;
        var dot = Vector.DotProduct(from, to);
        if (dot > 1.0 - 1e-6)
            return Identity;
        else if (dot < -(1.0 - 1e-6))
            return new Quaternion(new Vector(0, 0, 1), Math.PI);

        var s = Math.Sqrt(2 * (1 + dot));
        var invs = 1.0 / s;
        return new Quaternion(
            (from.Y * to.Z - from.Z * to.Y) * invs,
            (from.Z * to.X - from.X * to.Z) * invs,
            (from.X * to.Y - from.Y * to.X) * invs,
            0.5 * s).Norm;
    }

    public bool Equals(Quaternion other)
        => X == other.X && Y == other.Y && Z == other.Z && W == other.W;

    public static readonly Quaternion Identity = new Quaternion(0, 0, 0, 1);
}

public class QuaternionJsonConverter : JsonConverter<Quaternion>
{
    public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
            return new Quaternion();
        var tokens = str.Split(',');
        return new Quaternion
        {
            X = double.Parse(tokens[0], CultureInfo.InvariantCulture),
            Y = double.Parse(tokens[1], CultureInfo.InvariantCulture),
            Z = double.Parse(tokens[2], CultureInfo.InvariantCulture),
            W = double.Parse(tokens[2], CultureInfo.InvariantCulture),
        };
    }

    public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
    {
        var str = string.Join(',',
            value.X.ToString(CultureInfo.InvariantCulture),
            value.Y.ToString(CultureInfo.InvariantCulture),
            value.Z.ToString(CultureInfo.InvariantCulture),
            value.W.ToString(CultureInfo.InvariantCulture)
            );
        writer.WriteStringValue(str);
    }
}
