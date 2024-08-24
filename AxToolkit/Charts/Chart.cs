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
using AxToolkit.Graphics;
using System.Data;
using System.Drawing;
using System.Globalization;

namespace AxToolkit.Charts;

public enum Positionements
{
    Left,
    Right,
    Top,
    Bottom,
}




public class ChartFontOptions
{
    public string Family { get; set; }
    public float? Size { get; set; }
    public string Variant { get; set; }
    public Color? Color { get; set; }

    public ChartFontOptions ResolveFont(ChartFontOptions font)
    {
        return new ChartFontOptions
        {
            Family = !string.IsNullOrEmpty(font.Family) ? font.Family : !string.IsNullOrEmpty(Family) ? Family : "Arial",
            Size = font.Size.HasValue ? font.Size.Value : Size.HasValue ? Size.Value : 12.0f,
            Color = font.Color.HasValue ? font.Color.Value : Color.HasValue ? Color.Value : System.Drawing.Color.Black,
            Variant = !string.IsNullOrEmpty(font.Variant) ? font.Variant : !string.IsNullOrEmpty(Variant) ? Variant : "regular",
        };
    }

}

public class ChartOptions
{
    public Color? BackgroundColor { get; set; }
    public bool ShowLegends { get; set; }
    public string Title { get; set; }
    public ChartFontOptions TitleFont { get; set; } = new ChartFontOptions();
    public ChartFontOptions Font { get; set; } = new ChartFontOptions();

    public Dictionary<string, ChartScaleOptions> ScaleOptions { get; } = new Dictionary<string, ChartScaleOptions>();
    public Box Padding { get; internal set; }

    public ChartFontOptions ResolveFont(ChartFontOptions font) => Font.ResolveFont(font);
}

public class ChartScaleOptions
{
    public float TicksLength { get; set; } = 5;
    public Box Padding { get; set; } = new Box(5);
    public Color BorderColor { get; set; } = Color.DarkGray;
    public float BorderWidth { get; set; } = 1.6f;
    public Color TicksColor { get; set; } = Color.Gray;
    public float TicksWidth { get; set; } = 1.0f;
    public float MinSpacing { get; set; } = 10;
    public ChartFontOptions TicksFont { get; set; } = new ChartFontOptions
    {
        Color = Color.DarkSlateGray,
        Size = 10,
    };
}


public class ChartDatasetOptions
{
    public Color? Color { get; set; }

    public float Width { get; set; } = 1.4f;
    public bool DrawPoints { get; set; } // TODO -- List point style
    public Color? FillColor { get; set; }

}


// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

struct Range
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

internal abstract class Dataset
{
    public ChartDatasetOptions Options { get; internal set; }
    public Color Color { get; internal set; }
    public string XName { get; internal set; }
    public string YName { get; internal set; }
    public string ZName { get; internal set; }

    internal bool UseScale(string name)
    {
        int idx = name == "x" ? 0 : name == "y" ? 1 : name == "z" ? 2 : 3;
        return idx != 3;
    }

    internal Range Range(string name)
    {
        var range = new Range();
        int idx = name == "x" ? 0 : name == "y" ? 1 : name == "z" ? 2 : 3;
        foreach (var val in Enumerate())
        {
            if (range.Min > val[idx])
                range.Min = val[idx];
            if (range.Max < val[idx])
                range.Max = val[idx];
        }
        return range;
    }

    internal abstract IEnumerable<decimal[]> Enumerate();
}

internal class ListDataset : Dataset
{
    public List<decimal[]> Values { get; set; }
    internal override IEnumerable<decimal[]> Enumerate() => Values;
}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

public class Chart : IScreen
{
    private readonly List<ChartComponent> _components = new List<ChartComponent>();
    private readonly List<ChartScale> _scales = new List<ChartScale>();
    private readonly List<Dataset> _datasets = new List<Dataset>();


    public Chart(ChartOptions options)
    {
        // Title 
        if (!string.IsNullOrEmpty(options.Title))
        {
            var label = new ChartLabel
            {
                Text = options.Title,
                FontOpts = options.ResolveFont(options.TitleFont),
                Positionement = Positionements.Top,
                Chart = this,
                Padding = new Box(),
            };
            _components.Add(label);
        }

        // Legend
        if (options.ShowLegends)
        {
            // TODO -- 
        }

        // Scales
        foreach (var kv in options.ScaleOptions)
            AddScale(kv.Key, kv.Value);

        Options = options;
    }

    public void AddScale(string name, ChartScaleOptions options)
    {
        var scale = new ChartScale
        {
            Font = options.TicksFont,
            Name = name,
            Options = options,
            Positionement = name == "x" ? Positionements.Bottom : Positionements.Left,
            Padding = options.Padding,
        };
        _scales.Add(scale);
    }


    public void AddDataset(string name, decimal[] values, ChartDatasetOptions options)
    {
        var dset = new ListDataset
        {
            Color = options.Color.Value,
            Options = options,
            XName = "c",
            YName = "y",
            Values = values.Select(x => new decimal[] { x }).ToList()
        };
        _datasets.Add(dset);
    }

    public void AddDataset(string name, IEnumerable<decimal[]> values, ChartDatasetOptions options)
    {
        var dset = new ListDataset
        {
            Color = options.Color.Value,
            Options = options,
            XName = "x",
            YName = "y",
            Values = values.ToList()
        };
        _datasets.Add(dset);
    }


    public float Width { get; private set; }
    public float Height { get; private set; }

    public void Resize(IDrawingContext ctx, float width, float height)
    {
        Resizing?.Invoke(this, EventArgs.Empty);
        Width = width;
        Height = height;
        var chartArea = new Box(0, 0, width, height);
        foreach (var component in _components)
        {
            if (component.Positionement == Positionements.Top)
            {
                var sz = component.ComputeSize(ctx, chartArea.Width, null);
                component.Position = new Box(chartArea.Left, chartArea.Top, sz);
                chartArea.Top += sz.Height;
            }
            else if (component.Positionement == Positionements.Bottom)
            {
                var sz = component.ComputeSize(ctx, chartArea.Width, null);
                component.Position = new Box(chartArea.Left, chartArea.Bottom - sz.Height, sz);
                chartArea.Bottom -= sz.Height;
            }
            else if (component.Positionement == Positionements.Left)
            {
                var sz = component.ComputeSize(ctx, null, chartArea.Height);
                component.Position = new Box(chartArea.Left, chartArea.Top, sz);
                chartArea.Left += sz.Width;
            }
            else if (component.Positionement == Positionements.Right)
            {
                var sz = component.ComputeSize(ctx, null, chartArea.Height);
                component.Position = new Box(chartArea.Right - sz.Width, chartArea.Top, sz);
                chartArea.Right -= sz.Width;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        ChartArea = chartArea;

        foreach (var scale in _scales)
        {
            var length = scale.IsHorizontal ? ChartArea.Width : ChartArea.Height;
            var sets = _datasets.Where(x => x.UseScale(scale.Name)).Select(x => x.Range(scale.Name));
            scale.ConfigureSize(ctx, length, sets.Min(x => x.Min), sets.Max(x => x.Max));
        }

        var margin = new Box
        {
            Left = Math.Max(_scales.Where(x => x.IsHorizontal).Max(x => x.Margin.Left), _scales.Where(x => x.IsLeft).Sum(x => x.Margin.SumWidth + x.MinSize.Width)),
            Right = Math.Max(_scales.Where(x => x.IsHorizontal).Max(x => x.Margin.Right), _scales.Where(x => x.IsRight).Sum(x => x.Margin.SumWidth + x.MinSize.Width)),
            Bottom = Math.Max(_scales.Where(x => !x.IsHorizontal).Max(x => x.Margin.Bottom), _scales.Where(x => x.IsHorizontal).Sum(x => x.Margin.SumHeight + x.MinSize.Height)),
            Top = _scales.Where(x => !x.IsHorizontal).Max(x => x.Margin.Top),
        };

        CurveArea = chartArea.Srink(margin);

        var area = CurveArea;
        foreach (var scale in _scales)
        {
            var sets = _datasets.Where(x => x.UseScale(scale.Name)).Select(x => x.Range(scale.Name));
            var sz = scale.ConfigureTicks(ctx, CurveArea, area, sets.Min(x => x.Min), sets.Max(x => x.Max));
            if (scale.IsLeft && sz.Left < area.Left)
                area.Left = sz.Left;
            if (scale.IsRight && sz.Right > area.Right)
                area.Right = sz.Right;
            if (scale.IsHorizontal && sz.Top < area.Top)
                area.Top = sz.Top;
            if (scale.IsHorizontal && sz.Bottom > area.Bottom)
                area.Bottom = sz.Bottom;
        }

        Resized?.Invoke(this, EventArgs.Empty);
    }



    public ChartOptions Options { get; set; }
    public Box ChartArea { get; private set; }
    public Box CurveArea { get; private set; }


    public event EventHandler? Resizing;
    public event EventHandler? Resized;



    public void Paint(IDrawingContext ctx)
    {
        if (Width != ctx.Width || Height != ctx.Height)
            Resize(ctx, (float)ctx.Width, (float)ctx.Height);

        ctx.Save();
        // Draw components
        foreach (var component in _components)
        {
            component.Paint(ctx);
        }

        // Draw curve background
        if (Options.BackgroundColor.HasValue)
        {
            ctx.FillStyle(Options.BackgroundColor.Value);
            ctx.BeginPath();
            ctx.Rect(CurveArea.Left, CurveArea.Top, CurveArea.Width, CurveArea.Height);
            ctx.Fill();
        }

        // Draw scale and ticks
        foreach (var scale in _scales)
        {
            scale.Paint(ctx);
        }

        // Draw datasets
        var ctrl = new ChartLineController();
        ctrl.XScale = _scales.Single(x => x.Name == "x");
        ctrl.YScale = _scales.Single(x => x.Name == "y");
        ctrl.Paint(ctx, CurveArea, _datasets, _scales);

        ctx.Restore();
    }

    public void MouseOver(double x, double y) =>  throw new NotImplementedException();

    public void MouseClick(double x, double y, int btn) =>  throw new NotImplementedException();
}


internal class ChartLineController
{
    public ChartScale XScale { get; set; }
    public ChartScale YScale { get; set; }

    public void Paint(IDrawingContext ctx, Box curveArea, IEnumerable<Dataset> datasets, IEnumerable<ChartScale> scales)
    {
        ctx.Save();
        // Clip curveArea
        foreach (var dset in datasets)
        {
            // Check
            Paint(ctx, dset);
        }
        // Clip curveArea
        ctx.Restore();
    }

    void Paint(IDrawingContext ctx, Dataset dataset)
    {
        var opt = dataset.Options;
        ctx.StrokeStyle(dataset.Color, opt.Width);
        if (opt.FillColor.HasValue)
            ctx.FillStyle(opt.FillColor.Value);
        var dr = false;
        var points = new List<(float, float)>();
        var y0 = YScale.PixelForValue(0);
        var lastPx = 0.0f;
        var firstPx = 0.0f;
        foreach (var pt in dataset.Enumerate())
        {
            if (pt == null)
            {
                if (dr)
                {
                    ctx.Stroke();

                    if (opt.FillColor.HasValue)
                    {
                        ctx.LineTo(lastPx, y0);
                        ctx.LineTo(firstPx, y0);
                        ctx.Fill();
                    }
                }
                dr = false;
                continue;
            }

            var px = XScale.PixelForValue(pt[0]);
            var py = YScale.PixelForValue(pt[1]);
            if (opt.DrawPoints)
                points.Add((px, py));

            if (!dr)
            {
                ctx.BeginPath();
                ctx.MoveTo(px, py);
                firstPx = px;
                lastPx = px;
                dr = true;
            }
            else
            {
                ctx.LineTo(px, py);
                lastPx = px;
            }
        }
        if (dr)
        {
            ctx.Stroke();

            if (opt.FillColor.HasValue)
            {
                ctx.LineTo(lastPx, y0);
                ctx.LineTo(firstPx, y0);
                ctx.Fill();
            }
        }

        // If Use Markee
        foreach (var pt in points)
        {
            // Draw !!
        }
    }
}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


class ChartTickItem
{
    public decimal Value { get; internal set; }
    public float Width { get; internal set; }
    public string Label { get; internal set; }
    public bool DrawGrid { get; internal set; }
}




internal class ChartScale
{
    public Positionements Positionement { get; set; }
    public bool IsHorizontal => Positionement == Positionements.Bottom;
    public bool IsLeft=> Positionement == Positionements.Left;
    public bool IsRight => Positionement == Positionements.Right;
    public string Name { get; set; }

    Box _chartArea;
    public float _length;
    NiceScale _niceScale;

    public ChartTickItem[] Ticks { get; private set; }

    public ChartScaleOptions Options { get; set; }
    public ChartFontOptions Font { get; set; }

    public Box Position { get; private set; }
    public Box Margin { get; private set; } = new Box();
    public Box Padding { get; set; }
    public SizeF MinSize { get; private set; } = new SizeF();


    //internal override SizeF ComputeSize(float? width, float? height)
    //{
    //    // TODO -- var fontFace = Chart.PaintContext.ResolveFont(Options.FontFamily, Options.FontSize, Options.FontVariant);
    //    var lineHeight = 12.0f; // fontFace.LineHeight
    //    if (IsHorizontal)
    //    {
    //        return new SizeF(width ?? Options.Padding.Width, Options.Padding.Height + lineHeight + Options.TicksLength);
    //    }
    //    else
    //    {
    //        var tickLabels = new string[2];
    //        var widths = new float[] { 60 };
    //        // TODO -- var widths = tickLabels.Select(x => Chart.PaintContext.MeasureText(fontFace, x).Width);
    //        return new SizeF(Options.Padding.Width + Options.TicksLength + widths.Max(), height ?? Options.Padding.Height + lineHeight);
    //    }
    //}

    internal void ConfigureSize(IDrawingContext ctx, float length, decimal min, decimal max)
    {
        var maxTicks = ComputeMaxTicks(length);
        var nice = new NiceScale(min, max, maxTicks);

        ctx.FontStyle(Font.Family, Font.Size.GetValueOrDefault(12), TextVariant.None);
        var mesure = ctx.MeasureText("A");
        var lineHeight = mesure.LineHeight;
        var baseline = mesure.Baseline;
        var wmin = ctx.MeasureText(LabelForValue(nice.MinPoint)).Width;
        var wmax = ctx.MeasureText(LabelForValue(nice.MaxPoint)).Width;
        if (IsHorizontal)
        {
            Margin = new Box
            {
                Left = Math.Max(wmin / 2, Padding.Left),
                Right = Math.Max(wmax / 2, Padding.Right),
                Top = Math.Max(Options.TicksLength, Padding.Top),
                Bottom = Padding.Bottom,
            };
            // Is there going to be several line of text
            MinSize = new SizeF
            {
                Width = (wmin + wmax) / 2 + Options.MinSpacing,
                Height = lineHeight,
            };
        }
        else
        {
            Margin = new Box
            {
                Top = Math.Max(baseline, Padding.Top),
                Bottom = Math.Max(lineHeight - baseline, Padding.Bottom),
                Left = Math.Max(0, Padding.Left),
                Right = Math.Max(0, Padding.Right),
            };
            MinSize = new SizeF
            {
                Width = Math.Max(wmin, wmax),
                Height = lineHeight + Options.MinSpacing,
            };
        }
    }


    internal Box ConfigureTicks(IDrawingContext ctx, Box chartArea, Box gridArea, decimal min, decimal max)
    {
        _chartArea = chartArea;
        _length = IsHorizontal ? chartArea.Width : chartArea.Height;
        var maxTicks = ComputeMaxTicks(_length);
        _niceScale = new NiceScale(min, max, maxTicks);

        Ticks = _niceScale.Enumerate().Select(x => {
            var label = LabelForValue(x);
            return new ChartTickItem
            {
                Value = x,
                DrawGrid = true,
                Label = label,
                Width = ctx.MeasureText(label).Width,
            };
        }).ToArray();

        if (IsHorizontal)
        {
            Position = new Box
            {
                Top = gridArea.Top,
                Left = chartArea.Left,
                Right = chartArea.Right,
                Bottom = gridArea.Top + MinSize.Height,
            };
        }
        else if (IsLeft)
        {
            Position = new Box
            {
                Top = chartArea.Top,
                Left = gridArea.Left - MinSize.Width,
                Right = gridArea.Left,
                Bottom = chartArea.Bottom,
            };
        }
        else if (IsRight)
        {
            Position = new Box
            {
                Top = chartArea.Top,
                Left = gridArea.Right,
                Right = gridArea.Right + MinSize.Width,
                Bottom = chartArea.Bottom,
            };
        }

        return Position;
    }


    private int ComputeMaxTicks(float length)
    {
        return Math.Max(2, Math.Min(40, (int)length / 40));
    }

    internal void Paint(IDrawingContext ctx)
    {
        ctx.FillStyle(Font.Color.GetValueOrDefault(Color.DarkGray));
        ctx.FontStyle(Font.Family, Font.Size.GetValueOrDefault(12), TextVariant.None);
        if (Positionement == Positionements.Left)
        {
            ctx.StrokeStyle(Options.BorderColor, Options.BorderWidth);
            ctx.BeginPath();
            ctx.MoveTo(_chartArea.Left, _chartArea.Top);
            ctx.LineTo(_chartArea.Left, _chartArea.Bottom);
            ctx.Stroke();

            ctx.StrokeStyle(Options.TicksColor, Options.TicksWidth);
            foreach (var tick in Ticks)
            {
                var px = PixelForValue(tick.Value);
                ctx.BeginPath();
                ctx.MoveTo(tick.DrawGrid ? _chartArea.Right : _chartArea.Left, px);
                ctx.LineTo(_chartArea.Left - Options.TicksLength, px);
                ctx.Stroke();
                ctx.Text(_chartArea.Left - Options.TicksLength - Options.Padding.Right - tick.Width, px, tick.Label);
            }
        }
        else if (Positionement == Positionements.Right)
        {
            ctx.StrokeStyle(Options.BorderColor, Options.BorderWidth);
            ctx.BeginPath();
            ctx.MoveTo(_chartArea.Right, _chartArea.Top);
            ctx.LineTo(_chartArea.Right, _chartArea.Bottom);
            ctx.Stroke();

            ctx.StrokeStyle(Options.TicksColor, Options.TicksWidth);
            foreach (var tick in Ticks)
            {
                var px = PixelForValue(tick.Value);
                ctx.BeginPath();
                ctx.MoveTo(tick.DrawGrid ? _chartArea.Left : _chartArea.Right, px);
                ctx.LineTo(_chartArea.Right + Options.TicksLength, px);
                ctx.Stroke();
                ctx.Text(_chartArea.Right + Options.TicksLength + Options.Padding.Left, px, tick.Label);
            }
        }
        else if (Positionement == Positionements.Bottom)
        {
            ctx.StrokeStyle(Options.BorderColor, Options.BorderWidth);
            ctx.BeginPath();
            ctx.MoveTo(_chartArea.Left, _chartArea.Bottom);
            ctx.LineTo(_chartArea.Right, _chartArea.Bottom);
            ctx.Stroke();

            ctx.StrokeStyle(Options.TicksColor, Options.TicksWidth);
            foreach (var tick in Ticks)
            {
                var px = PixelForValue(tick.Value);
                ctx.BeginPath();
                ctx.MoveTo(px, tick.DrawGrid ? _chartArea.Top : _chartArea.Bottom);
                ctx.LineTo(px, _chartArea.Bottom + Options.TicksLength);
                ctx.Stroke();
                ctx.Text(px - tick.Width / 2, _chartArea.Bottom + Options.TicksLength + Options.Padding.Top, tick.Label);
            }
        }
        else 
            throw new InvalidOperationException();
    }

    internal float PixelForValue(decimal value)
    {
        if (value < _niceScale.NiceMin || value > _niceScale.NiceMax)
            return float.NaN;

        var place = value - _niceScale.NiceMin;
        var ratio = (float)(place / _niceScale.FinalRange);
        if (IsHorizontal)
            return ratio * _length + _chartArea.Left;
        else
            return _chartArea.Bottom - ratio * _length;
    }


    public virtual string LabelForValue(decimal value)
    {
        return value.ToString("0", new CultureInfo("fr"));
    }

}



// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=



internal class ChartLabel : ChartComponent
{
    public string Text { get; set; }

    public Box Padding { get; set; }

    public ChartFontOptions FontOpts { get; set; }
    internal override SizeF ComputeSize(IDrawingContext ctx, float? width, float? height)
    {
        ctx.FontStyle(FontOpts.Family, FontOpts.Size.GetValueOrDefault(12), TextVariant.None);
        var measure = ctx.MeasureText(Text);
        return new SizeF(measure.Width + Padding.Width, measure.LineHeight + Padding.Height);
    }

    internal override void Paint(IDrawingContext ctx)
    {
        // var fontFace = ctx.ResolveFont(FontOpts.Family, FontOpts.Size, FontOpts.Variant);
        ctx.FontStyle(FontOpts.Family, FontOpts.Size.GetValueOrDefault(12), TextVariant.None);
        var baseline = ctx.MeasureText("a").Baseline;
        ctx.FillStyle(FontOpts.Color.GetValueOrDefault(Color.Black));
        ctx.Text(Position.Left + Padding.Left, Position.Top + Padding.Top + baseline, Text);
    }
}



internal abstract class ChartComponent
{
    public Chart Chart { get; internal set; }
    public Box Position { get; internal set; }
    public Positionements Positionement { get; internal set; }

    internal abstract SizeF ComputeSize(IDrawingContext ctx, float? width, float? height);
    internal abstract void Paint(IDrawingContext ctx);
}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

#if false
internal class ChartBaromptroller
{
    public Chart Chart { get; }
    public void Paint()
    {
        //var ctx = Chart.PaintContext;
        //var step = chartArea.Width / DataLength();
        //var gap = 5;
        //var barWidth = Math.Max(((step - gap) / DataCount() - gap), gap);


        // If <decimal?> : Draw Bar from zero to Value
        // If <(decimal, decimal)> : Draw Bar from min to max (2 Colors ?)            
        // Else More Complex draw candles...

        // If Horizontal  ..
        // Is Stacked (DataCount = by groups)

        // X Must be INTEGER
    }
}

internal class ChartLineComptroller
{
    // BARS:     X:INTEGER, Y:DECIMAL
    // BARS:     X:INTEGER, Y:DECIMAL[] ...
    // BARS-STACK  X:INTEGER, Y:DECIMAL   (Groped dataset)
    // LINES:    X:INTEGER, Y:DECIMAL?
    // LINE STACK  X:INTEGER, Y:DECIMAL


    // LINES:    X:DECIMAL Y:DECIMAL?
    // SCATTER:   X:DECIMAL Y:DECIMAL
    // BUBBLES:   X:DECIMAL Y:DECIMAL, Z:DECIMAL

    // PIE   V:DECIMAL   (1 Dataset)
    // MULTIPIE     V:DECIMAL 
    // POLAR/RADAR   V:DECIMAL

    public List<ChartDataset> Datasets { get; set; }

    public ChartScale XScale { get; set; }
    public ChartScale YScale { get; set; }

    public Chart Chart { get; }

    public void Configure()
    {

        // CATEGORY
        var xmin = 0;
        var xmax = Datasets.Max(x => x.Count());

        var ymin = Datasets.Min(x => x.Min());
        var ymax = Datasets.Max(x => x.Max());

        // Configure Scale 
    }

}

internal abstract class ChartBaseController
{
    public ChartScale XScale { get; set; }
    public ChartScale YScale { get; set; }
    public ChartScale ZScale { get; set; }

    public abstract void Draw(IDrawer ctx, ChartDatasetOptions options, IEnumerable<decimal[]?> list);
}

internal class ChartBarController : ChartBaseController
{
    public override void Draw(IDrawer ctx, ChartDatasetOptions options, IEnumerable<decimal[]?> list)
    {
        var barLeft = 0.0f; // (width / count - 2 * gap) / 2; (unless several dataset/group)
        var barWidth = 0.0f;
        var p0 = YScale.PixelForValue(0);
        foreach (var pt in list)
        {
            if (pt == null)
                continue;

            var px = XScale.PixelForValue(pt[0]);
            var py = YScale.PixelForValue(pt[1]);

            ctx.BeginPath();
            ctx.Rect(px - barLeft, p0, barWidth, p0 - py); // If stacked, use [MIN,MAX] unstead of y0
            ctx.Fill();
            ctx.Stroke();
        }
    }
}

internal class ChartFloatingBarController : ChartBaseController
{
    public override void Draw(IDrawer ctx, ChartDatasetOptions options, IEnumerable<decimal[]?> list)
    {
        var barLeft = 0.0f; // (width / count - 2 * gap) / 2; (unless several dataset/group)
        var barWidth = 0.0f;
        var p0 = YScale.PixelForValue(0);
        foreach (var pt in list)
        {
            if (pt == null)
                continue;

            var px = XScale.PixelForValue(pt[0]);
            var py1 = YScale.PixelForValue(pt[1]);
            var py2 = YScale.PixelForValue(pt[2]);

            ctx.BeginPath();
            ctx.Rect(px - barLeft, py1, barWidth, py1 - py2); // If stacked, use [MIN,MAX] unstead of y0
            ctx.Fill();
            ctx.Stroke();
        }
    }
}

internal class ChartLineCategoryController : ChartLineController
{
    public override void Draw(IDrawer ctx, ChartDatasetOptions options, IEnumerable<decimal[]?> list)
    {
        base.Draw(ctx, options, list.Select((x, i) => x != null ? new decimal[] { i, x[0] } : null));
    }
}

internal class ChartLineController2 : ChartBaseController
{

    public override void Draw(IDrawer ctx, ChartDatasetOptions options, IEnumerable<decimal[]?> list)
    {
        ctx.StrokeStyle(ColorExt.Parse(options.Color), options.Width);
        if (options.FillArea)
            ctx.FillStyle(ColorExt.Parse(options.FillColor));
        var dr = false;
        var points = new List<(float, float)>();
        var y0 = YScale.PixelForValue(0);
        var lastPx = 0.0;
        var firstPx = 0.0;
        foreach (var pt in list)
        {
            if (pt == null)
            {
                if (dr)
                {
                    ctx.Stroke();

                    if (options.FillArea)
                    {
                        ctx.LineTo(lastPx, y0);
                        ctx.LineTo(firstPx, y0);
                        ctx.Fill();
                    }
                }
                dr = false;
                continue;
            }

            var px = XScale.PixelForValue(pt[0]);
            var py = YScale.PixelForValue(pt[1]);
            if (options.DrawPoints)
                points.Add((px, py));

            if (!dr)
            {
                ctx.BeginPath();
                ctx.MoveTo(px, py);
                firstPx = px;
                lastPx = px;
                dr = true;
            }
            else
            {
                ctx.LineTo(px, py);
                lastPx = px;
            }
        }
        if (dr)
        {
            ctx.Stroke();

            if (options.FillArea)
            {
                ctx.LineTo(lastPx, y0);
                ctx.LineTo(firstPx, y0);
                ctx.Fill();
            }
        }

        // If Use Markee
        foreach (var pt in points)
        {
            // Draw !!
        }
    }
}

internal class ChartScatterController : ChartBaseController
{
    public override void Draw(IDrawer ctx, ChartDatasetOptions options, IEnumerable<decimal[]?> list)
    {
        ctx.StrokeStyle(ColorExt.Parse(options.Color), options.Width);
        if (options.FillArea)
            ctx.FillStyle(ColorExt.Parse(options.FillColor));
        foreach (var pt in list)
        {
            if (pt == null)
                continue;

            var px = XScale.PixelForValue(pt[0]);
            var py = YScale.PixelForValue(pt[1]);

            ctx.BeginPath();
            ctx.Arc(px, py, options.Width, 0, 2 * Math.PI);
            ctx.Fill();
            ctx.Stroke();
        }
    }
}
internal class ChartBubbleController : ChartBaseController
{
    public override void Draw(IDrawer ctx, ChartDatasetOptions options, IEnumerable<decimal[]?> list)
    {
        ctx.StrokeStyle(ColorExt.Parse(options.Color), options.Width);
        if (options.FillArea)
            ctx.FillStyle(ColorExt.Parse(options.FillColor));
        foreach (var pt in list)
        {
            if (pt == null)
                continue;

            var px = XScale.PixelForValue(pt[0]);
            var py = YScale.PixelForValue(pt[1]);
            var pz = ZScale.PixelForValue(pt[2]);

            ctx.BeginPath();
            ctx.Arc(px, py, pz, 0, 2 * Math.PI);
            ctx.Fill();
            ctx.Stroke();
        }
    }
}
#endif
