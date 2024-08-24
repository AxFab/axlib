using AxToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Mathematics.Physics;

public static class MovingObject
{
    public class CollitionData
    {
        public Vec2 Normal { get; set; }
        public double Force { get; set; }
        public double Impact1 { get; set; }
        public double Impact2 { get; set; }
    }
    public static double Collision2CirclesDetect(ICircleMovingObject u1, ICircleMovingObject u2)
    {
        var pos = u1.Position - u2.Position;
        var spd = u1.Speed - u2.Speed;

        var a = Vec2.DotProduct(spd, spd);
        var b = 2 * Vec2.DotProduct(pos, spd);
        var c = Vec2.DotProduct(pos, pos) - Math.Pow(u1.Radius + u2.Radius, 2);

        var discriminent = b * b - 4 * a * c;
        if (a == 0 || discriminent < 0)
            return double.NaN;
        var v1 = -(b - Math.Sqrt(discriminent)) / (2 * a);
        var v2 = -(b + Math.Sqrt(discriminent)) / (2 * a);
        var res = Math.Min(v1, v2);
        return res >= 0 ? res : double.NaN;
    }
    public static CollitionData Collision2CirclesResolve(ICircleMovingObject u1, ICircleMovingObject u2, double minImpact)
    {
        var pos = u1.Position - u2.Position;

        var normal = pos.Norm();
        var p = 2 * (Vec2.DotProduct(u1.Speed, normal) - Vec2.DotProduct(u2.Speed, normal)) / (u1.Mass + u2.Mass);

        var imp1 = p * u1.Mass;
        if (Math.Abs(imp1) < minImpact)
            imp1 *= minImpact / Math.Abs(imp1);

        var imp2 = p * u2.Mass;
        if (Math.Abs(imp2) < minImpact)
            imp2 *= minImpact / Math.Abs(imp2);

        u1.Speed -= imp1 * normal;
        u2.Speed += imp2 * normal;

        return new CollitionData
        {
            Normal = normal,
            Force = p,
            Impact1 = imp1,
            Impact2 = imp2,
        };
    }

}
