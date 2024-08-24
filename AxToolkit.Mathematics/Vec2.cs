using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Mathematics;

public struct Vec2
{
    public Vec2()
    {
        X = 0; Y = 0;
    }
    public Vec2(double x, double y)
    {
        X = x; Y = y;
    }
    public double X { get; set; }
    public double Y { get; set; }

    public double LengthSq => X * X + Y * Y;
    public double Length => Math.Sqrt(X * X + Y * Y);
    public Vec2 Norm(double radius = 1.0)
    {
        if (LengthSq == 0)
            return new Vec2();
        var len = radius / Length;
        return new Vec2(X * len, Y * len);
    }

    public double Angle
    {
        get
        {
            var angle = Math.Atan2(Y, X) * 180 / Math.PI;
            if (angle < 0)
                angle += 360;
            return angle;
        }
    }
    public double Argument => Math.Atan2(Y, X);

    public static Vec2 Zero => new Vec2(0, 0);
    public static Vec2 One => new Vec2(1, 1);
    public static Vec2 Up => new Vec2(0, -1);
    public static Vec2 Down => new Vec2(0, 1);
    public static Vec2 Left => new Vec2(-1, 0);
    public static Vec2 Right => new Vec2(1, 0);

    public static Vec2 operator +(Vec2 v1, Vec2 v2) => new Vec2(v1.X + v2.X, v1.Y + v2.Y);
    public static Vec2 operator -(Vec2 v1, Vec2 v2) => new Vec2(v1.X - v2.X, v1.Y - v2.Y);
    public static Vec2 operator *(Vec2 v1, double k) => new Vec2(v1.X * k, v1.Y * k);
    public static Vec2 operator *(double k, Vec2 v1) => new Vec2(v1.X * k, v1.Y * k);
    public static Vec2 operator /(Vec2 v1, double k) => new Vec2(v1.X / k, v1.Y / k);

    public static bool operator <=(Vec2 v1, double k) => v1.LengthSq <= k * k;
    public static bool operator >=(Vec2 v1, double k) => v1.LengthSq >= k * k;
    public static bool operator <(Vec2 v1, double k) => v1.LengthSq < k * k;
    public static bool operator >(Vec2 v1, double k) => v1.LengthSq > k * k;

    public string ToIntString => $"{X:0} {Y:0}";

    public override string ToString() => $"{X:0.0} {Y:0.0}";

    public Vec2 XYSwap() => new Vec2(Y, X);

    public static double AngleUpdate(double origin, double target, double max)
    {
        var o = origin + 360;
        var t = target - origin;
        while (Math.Abs(target - o) > 180)
            o -= 360;
        var diff = target - origin;
        if (diff < -max)
            diff = -max;
        if (diff > max)
            diff = max;
        var n = origin + diff;
        if (n < 0)
            n += 360;
        if (n > 360)
            n -= 360;
        return n;
    }

    public static double AngleDiff(double target, double source)
    {
        var a = (target - source);
        a += 180;
        a = a - Math.Floor(a / 360.0) * 360.0;
        a -= 180;
        return a;
    }
    public static Vec2 FromAngleAndLength(double angle, double radius)
    {
        var arg = angle * Math.PI / 180;
        return new Vec2(radius * Math.Cos(arg), radius * Math.Sin(arg));
    }
    public static Vec2 FromArgAndLength(double arg, double radius)
    {
        return new Vec2(radius * Math.Cos(arg), radius * Math.Sin(arg));
    }

    public Vec2 Truncate() => new Vec2(Math.Truncate(X), Math.Truncate(Y));
    public Vec2 Round() => new Vec2(Math.Round(X), Math.Round(Y));
    public Vec2 Floor() => new Vec2(Math.Floor(X), Math.Floor(Y));
    public Vec2 Ceiling() => new Vec2(Math.Ceiling(X), Math.Ceiling(Y));

    public Vec2 Rotate(double arg) => new Vec2(Math.Cos(arg) * X - Math.Sin(arg) * Y, Math.Sin(arg) * X + Math.Cos(arg) * Y);
    public Vec2 RotateAngle(double angle) => Rotate(angle * Math.PI / 180);

    public static double DotProduct(Vec2 v1, Vec2 v2) => v1.X * v2.X + v1.Y * v2.Y;

    public Vec2 AsIntVec() => new Vec2((int)X, (int)Y);
}
