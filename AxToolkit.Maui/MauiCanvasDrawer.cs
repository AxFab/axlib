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
using AxToolkit.Mathematics;

namespace AxToolkit.Maui;

// All the code in this file is included in all platforms.
public class MauiCanvasDrawer : IDrawingContext
{
    private Color _fillColor = Colors.Black;
    private Color _strokeColor = Colors.Black;
    private float _strokeWidth = 1.0f;

    public MauiCanvasDrawer(ICanvas canvas, RectF dirtyRect)
    {
        Canvas = canvas;
        Width = dirtyRect.Width;
        Height = dirtyRect.Height;
    }

    public float Width { get; }
    public float Height { get; }
    public ICanvas Canvas { get; }

    public void Arc(float x, float y, float radius, float start, float end)
    {
        var path = CurrentPath();
        var angleStart = AxMath.Radian2Degree(-start);
        var angleEnd = AxMath.Radian2Degree(-end);
        path.AddArc((x - radius), (y - radius), (x + radius), (y + radius), angleStart, angleEnd, true);
    }

    public void Rect(float x, float y, float width, float height, float rx, float ry)
    {
        var path = CurrentPath();
        path.AppendRectangle(x, y, width, height);
    }

    public void Text(float x, float y, string value)
    {
        Canvas.FontColor = _fillColor;
        Canvas.DrawString(value, x, y, HorizontalAlignment.Left);
    }

    public void Text(float x, float y, string value, TextAlignement align)
    {
        Canvas.FontColor = _fillColor;
        Canvas.DrawString(value, x, y, HorizontalAlignment.Left);
    }


    public void BeginPath()
    {
        _path = null;
    }

    public void ClosePath()
    {
        var path = CurrentPath();
        path.Close();
    }

    public void MoveTo(float x, float y)
    {
        var path = CurrentPath();
        path.MoveTo(x, y);
    }

    public void LineTo(float x, float y)
    {
        var path = CurrentPath();
        path.LineTo(x, y);
    }
    public void QuadTo(float x1, float y1, float x, float y)
    {
        var path = CurrentPath();
        path.QuadTo(x1, y1, x, y);
    }
    public void CurveTo(float x1, float y1, float x2, float y2, float x, float y)
    {
        var path = CurrentPath();
        path.CurveTo(x1, y1, x2, y2, x, y);
    }

    private PathF _path;
    private PathF CurrentPath()
    {
        _path ??= new PathF();
        return _path;
    }


    public void Fill()
    {
        Canvas.Alpha = _fillColor.Alpha;
        Canvas.FillColor = _fillColor;
        var path = CurrentPath();
        Canvas.FillPath(path);
    }

    public void FillStyle(System.Drawing.Color color)
    {
        _fillColor = new Color(color.R, color.G, color.B, color.A);
    }

    public void Stroke()
    {
        Canvas.StrokeColor = _strokeColor;
        Canvas.StrokeSize = _strokeWidth;
        var path = CurrentPath();
        Canvas.DrawPath(path);
    }

    public void StrokeStyle(System.Drawing.Color color, float width)
    {
        _strokeColor = new Color(color.R, color.G, color.B);
        _strokeWidth = width;
    }

    public void Restore()
    {
        Canvas.RestoreState();
    }

    public void Save()
    {
        Canvas.SaveState();
    }

    public void Translate(float x, float y)
    {
        Canvas.Translate(x, y);
    }
    public void Rotate(float arg)
    {
        Canvas.Rotate(AxMath.Radian2Degree(arg));
    }

    public void Scale(float x, float y)
    {
        Canvas.Scale(x, y);
    }

    private Microsoft.Maui.Graphics.Font _font;
    private float _fontSize;
    public void FontStyle(string family, float size, TextVariant variant)
    {
        var weight = variant.HasFlag(TextVariant.Bold) ? 600 : 400;
        var style = variant.HasFlag(TextVariant.Italic) ? FontStyleType.Italic : FontStyleType.Normal;

        _font = new Microsoft.Maui.Graphics.Font(family, weight, style);
        _fontSize = size;
        Canvas.Font = _font;
        Canvas.FontSize = _fontSize;
    }

    public FontMeasure MeasureText(string text)
    {
        var asize = Canvas.GetStringSize("A", _font, _fontSize);
        var psize = Canvas.GetStringSize("p", _font, _fontSize);
        var fsize = Canvas.GetStringSize("Ap", _font, _fontSize);
        var size = Canvas.GetStringSize(text, _font, _fontSize);

        return new FontMeasure
        {
            Width = size.Width,
            Baseline = asize.Height,
            LineHeight = _fontSize,
            Ascender = asize.Height,
            Descender = size.Height - asize.Height,
        };
    }

}
