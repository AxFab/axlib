using AxToolkit.Graphics;
using Microsoft.Maui;

namespace AxMaui
{
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

            var angleStart = (float)(-start * 180 / Math.PI);
            var angleEnd = (float)(-end * 180 / Math.PI);
            path.AddArc((float)(x - radius), (float)(y - radius), (float)(x + radius), (float)(y + radius), angleStart, angleEnd, true);

            // _path.AddArc(pt.X - radius, pt.Y - radius, 2 * radius, 2 * radius, (float)(start * 180 / Math.PI), (float)((end - start) * 180 / Math.PI));

        }

        public void Rect(float x, float y, float width, float height, float rx = 0, float ry = 0)
        {
            var path = CurrentPath();
            path.AppendRectangle((float)x, (float)y, (float)width, (float)height);
        }

        public void Text(float x, float y, string text)
        {
            Canvas.FontColor = _fillColor;
            Canvas.DrawString(text, (float)x, (float)y, HorizontalAlignment.Left);
        }

        public void Text(float x, float y, string text, TextAlignement align = TextAlignement.Left)
        {
            Canvas.FontColor = _fillColor;
            Canvas.DrawString(text, (float)x, (float)y, HorizontalAlignment.Left);
            return;

            if (align == TextAlignement.Left)
                Canvas.DrawString(text, (float)x, (float)y, 500, 500, HorizontalAlignment.Left, VerticalAlignment.Top);
            else if (align == TextAlignement.Center)
                Canvas.DrawString(text, (float)x - 250, (float)y, 500, 500, HorizontalAlignment.Center, VerticalAlignment.Top);
            else if (align == TextAlignement.Right)
                Canvas.DrawString(text, (float)x - 500, (float)y, 500, 500, HorizontalAlignment.Right, VerticalAlignment.Top);
            else if (align == (TextAlignement.Left | TextAlignement.Bottom))
                Canvas.DrawString(text, (float)x, (float)y - 50, 500, 50, HorizontalAlignment.Left, VerticalAlignment.Bottom);
            else if (align == (TextAlignement.Center | TextAlignement.Bottom))
                Canvas.DrawString(text, (float)x - 250, (float)y - 50, 500, 50, HorizontalAlignment.Center, VerticalAlignment.Bottom);
            else if (align == (TextAlignement.Right | TextAlignement.Bottom))
                Canvas.DrawString(text, (float)x - 500, (float)y - 50, 500, 50, HorizontalAlignment.Right, VerticalAlignment.Bottom);
            else if (align == (TextAlignement.Left | TextAlignement.Middle))
                Canvas.DrawString(text, (float)x, (float)y - 25, 500, 50, HorizontalAlignment.Left, VerticalAlignment.Center);
            else if (align == (TextAlignement.Center | TextAlignement.Middle))
                Canvas.DrawString(text, (float)x - 250, (float)y - 25, 500, 50, HorizontalAlignment.Center, VerticalAlignment.Center);
            else if (align == (TextAlignement.Right | TextAlignement.Middle))
                Canvas.DrawString(text, (float)x - 500, (float)y - 25, 500, 50, HorizontalAlignment.Right, VerticalAlignment.Center);
            else
                throw new NotImplementedException();
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
            path.MoveTo((float)x, (float)y);
        }

        public void LineTo(float x, float y)
        {
            var path = CurrentPath();
            path.LineTo((float)x, (float)y);
        }
        public void QuadTo(float x1, float y1, float x, float y)
        {
            var path = CurrentPath();
            path.QuadTo((float)x1, (float)y1, (float)x, (float)y);
        }
        public void CurveTo(float x1, float y1, float x2, float y2, float x, float y)
        {
            var path = CurrentPath();
            path.CurveTo((float)x1, (float)y1, (float)x2, (float)y2, (float)x, (float)y);
        }

        private PathF _path;
        private PathF CurrentPath()
        {
            if (_path == null)
                _path = new PathF();
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

        public void StrokeStyle(System.Drawing.Color color, float width = 1.0f)
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
            Canvas.Translate((float)x, (float)y);
        }
        public void Rotate(float arg)
        {
            Canvas.Rotate((float)(arg * 180 / Math.PI));
        }

        public void Scale(float x, float y)
        {
            Canvas.Scale((float)x, (float)y);
        }

        private TextVariant _fontVariant;
        private Microsoft.Maui.Graphics.Font _font;
        private float _fontSize;
        public void FontStyle(string family, float size, TextVariant variant = TextVariant.None)
        {
            var weight = 400;
            var style = FontStyleType.Normal;
            
            
            _font = new Microsoft.Maui.Graphics.Font(family, weight, style);
            _fontSize = size;
            _fontVariant = variant;
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
}
