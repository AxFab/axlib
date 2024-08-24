using AxToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Mathematics.Physics;

public interface ICircleMovingObject
{
    public Vec2 Position { get; }
    public Vec2 Speed { get; set; }
    public double Radius { get; }
    public double Mass { get; }
}
