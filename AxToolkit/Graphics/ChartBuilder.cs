using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AxToolkit.Graphics
{
    public struct GBoundary
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }

        public GBoundary XV(double minx, double maxx)
        {
            return new GBoundary
            {
                MinX = minx,
                MaxX = maxx,
                MinY = MinY,
                MaxY = MaxY,
            };
        }
        public GBoundary YV(double miny, double maxy)
        {
            return new GBoundary
            {
                MinX = MinX,
                MaxX = MaxX,
                MinY = miny,
                MaxY = maxy,
            };
        }

        public GBoundary Add(double x, double y)
        {
            return new GBoundary
            {
                MinX = Math.Min(MinX, x),
                MaxX = Math.Max(MaxX, x),
                MinY = Math.Min(MinY, y),
                MaxY = Math.Max(MaxY, y),
            };
        }

        public static GBoundary Invalid => new GBoundary
        {
            MinX = double.MaxValue,
            MaxX = double.MinValue,
            MinY = double.MaxValue,
            MaxY = double.MinValue,
        };
        public static GBoundary New => new GBoundary
        {
            MinX = 0,
            MaxX = 0,
            MinY = 0,
            MaxY = 0,
        };

    }

    public class ChartBuilder
    {
        interface IChartLine
        {
            string Name { get; }
            Color Color { get; }
            float Tickness { get; }

            IEnumerator Enumerator();
            (double, double)? Value(object item);
        }
        class ChartLine<TLine> : IChartLine
        {
            public string Name { get; set; }
            public Color Color { get; set; }
            public float Tickness { get; set;  }
            public Func<IEnumerable<TLine>> Accessor { get; set; }
            public Func<TLine, (double, double)?> Function { get; set; }
            public IEnumerator Enumerator() => Accessor().GetEnumerator();
            public (double, double)? Value(object item) => Function((TLine)item);
        }

        class ChartGrid
        {
            public ChartGrid(double origin, double? step, Color color, float width)
            {
                Origin = origin;
                Step = step;
                Color = color;
                Width = width;
            }

            public double Origin { get; }
            public double? Step { get; }
            public Color Color { get; }
            public float Width { get; }

            public virtual IEnumerable<(double, double)> Enumerate(double min, double max, double size)
            {
                if (Origin >= min && Origin <= max)
                {
                    yield return (Origin, (Origin - min) * size / (max - min));
                }

                if (Step == null)
                    yield break;

                var lower = Origin - Step.Value;
                while (lower >= min)
                {
                    yield return (lower, (lower - min) * size / (max - min));
                    lower -= Step.Value;
                }

                var upper = Origin + Step.Value;
                while (upper <= max)
                {
                    yield return (upper, (upper - min) * size / (max - min));
                    upper += Step.Value;
                }
            }
        }

        private readonly List<IChartLine> _lines = new List<IChartLine>();
        private readonly List<ChartGrid> _gridX = new List<ChartGrid>();
        private readonly List<ChartGrid> _gridY = new List<ChartGrid>();
        public string Name { get; }

        public GBoundary Bounds { get; private set; }
        public ChartBuilder(string name)
        {
            Name = name;
        }

        public void SetBoundX(double min, double max)
        {
            Bounds = Bounds.XV(min, max);
        }
        public void SetBoundY(double min, double max)
        {
            Bounds = Bounds.YV(min, max);
        }

        public ChartBuilder Add<TLine>(string name, Color color, float tickness, Func<IEnumerable<TLine>> accessor, Func<TLine, (double, double)?> function)
        {
            _lines.Add(new ChartLine<TLine>
            {
                Name = name,
                Color = color,
                Tickness = tickness,
                Accessor = accessor,
                Function = function,
            });
            return this;
        }

        public ChartBuilder ClearGridX()
        {
            _gridX.Clear();
            return this;
        }
        public ChartBuilder AddGridX(double origin, Color color, float width)
        {
            _gridX.Add(new ChartGrid(origin, null, color, width));
            return this;
        }

        public ChartBuilder AddGridX(double origin, double step, Color color, float width)
        {
            _gridX.Add(new ChartGrid(origin, step, color, width));
            return this;
        }

        public ChartBuilder ClearGridY()
        {
            _gridY.Clear();
            return this;
        }
        public ChartBuilder AddGridY(double origin, Color color, float width)
        {
            _gridY.Add(new ChartGrid(origin, null, color, width));
            return this;
        }

        public ChartBuilder AddGridY(double origin, double step, Color color, float width)
        {
            _gridY.Add(new ChartGrid(origin, step, color, width));
            return this;
        }

        public void Drawing(IDrawingContext ctx)
        {
            int width = (int)ctx.Width;
            int height = (int)ctx.Height;
            int left = 60;
            int top = 30;
            int right = 30;
            int bottom = 30;
            int gp = 5;

            float viewWidth = width - left - right;
            float viewHeight = height - top - bottom;

            float ox = left + (float)(Bounds.MinX >= 0 ? 0 : -Bounds.MinX * viewWidth / (Bounds.MaxX - Bounds.MinX));
            float oy = top + viewHeight - (float)(Bounds.MinY >= 0 ? 0 : -Bounds.MinY * viewHeight / (Bounds.MaxY - Bounds.MinY));

            float aX = viewWidth / (float)(Bounds.MaxX - Bounds.MinX);
            float aY = -viewHeight / (float)(Bounds.MaxY - Bounds.MinY);
            float bX = -(float)Bounds.MinX * aX;
            float bY = -(float)Bounds.MinY * aY;


            //ctx.BeginPath();
            //ctx.Rect(left, top, viewWidth, viewHeight);
            //ctx.Fill(Color.FromArgb(242, 242, 242));
            ctx.FontStyle("Tahoma", 18, TextVariant.None);
            ctx.FillStyle(Color.White);
            ctx.Text(ctx.Width / 2, 5, Name, TextAlignement.Center);

            // X Grid
            foreach (var grid in _gridX)
            {
                foreach (var dx in grid.Enumerate(Bounds.MinX, Bounds.MaxX, viewWidth))
                {
                    var gx = left + (float)dx.Item2;
                    ctx.BeginPath();
                    ctx.MoveTo(gx, top + viewHeight);
                    ctx.LineTo(gx, top);
                    ctx.Stroke(grid.Color, grid.Width);

                    ctx.Text(gx, oy, dx.Item1.ToString(), TextAlignement.Center);
                    ctx.Fill(grid.Color);
                }
            }

            // X Axis
            ctx.BeginPath();
            ctx.MoveTo(left - gp, oy);
            ctx.LineTo(left + viewWidth, oy);
            ctx.Stroke(Color.FromArgb(240, 240, 240), 1.8f);

            // Y Grid
            foreach (var grid in _gridY)
            {
                foreach (var dy in grid.Enumerate(Bounds.MinY, Bounds.MaxY, viewHeight))
                {
                    var gy = top + viewHeight - (float)dy.Item2;
                    ctx.BeginPath();
                    ctx.MoveTo(left, gy);
                    ctx.LineTo(left + viewWidth, gy);
                    ctx.Stroke(grid.Color, grid.Width);

                    ctx.Text(left - 3, gy, dy.Item1.ToString(), TextAlignement.Right | TextAlignement.Middle);
                    ctx.Fill(grid.Color);
                }
            }

            // Y Axis
            ctx.BeginPath();
            ctx.MoveTo(ox, top + viewHeight + gp);
            ctx.LineTo(ox, top);
            ctx.Stroke(Color.FromArgb(240, 240, 240), 1.8f);

            // Functions
            var viewBottom = top + viewHeight;
            foreach (var curve in _lines)
            {
                bool readyDraw = false;
                var iterator = curve.Enumerator();
                while (iterator.MoveNext())
                {
                    var point = curve.Value(iterator.Current);
                    if (point == null || double.IsNaN(point.Value.Item1) || double.IsNaN(point.Value.Item2))
                    {
                        if (readyDraw)
                            ctx.Stroke(curve.Color, curve.Tickness);
                        readyDraw = false;
                        continue;
                    }
                    if (readyDraw)
                        ctx.LineTo(left + (float)point.Value.Item1 * aX + bX, viewBottom + (float)point.Value.Item2 * aY + bY);
                    else
                    {
                        ctx.BeginPath();
                        ctx.MoveTo(left + (float)point.Value.Item1 * aX + bX, viewBottom + (float)point.Value.Item2 * aY + bY);
                        readyDraw = true;
                    }
                }
                ctx.Stroke(curve.Color, curve.Tickness);
            }
        }
    }

}
