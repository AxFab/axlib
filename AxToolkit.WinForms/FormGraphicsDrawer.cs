﻿using AxToolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AxToolkit.WinForms
{
    [SupportedOSPlatform("windows6.1")]
    public class FormGraphicsDrawer : IDrawingContext
    {
        private readonly System.Drawing.Graphics _graphics;
        private readonly Stack<GraphicsState> _saveStack = new Stack<GraphicsState>();
        private GraphicsPath? _path;
        private PointF _cursor;
        private string _fontFamily;
        private float _fontSize;
        public FormGraphicsDrawer(System.Drawing.Graphics graphics, float x, float y, float width, float height)
        {
            _graphics = graphics;
            _graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            _graphics.SmoothingMode = SmoothingMode.AntiAlias;
            _graphics.CompositingQuality = CompositingQuality.HighQuality;
            _graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            _graphics.SetClip(new RectangleF(x, y, width, height));
            Width = width;
            Height = height;
            Translate(x, y);
        }
        public FormGraphicsDrawer(System.Drawing.Graphics graphics, RectangleF size)
            : this(graphics, size.X, size.Y, size.Width, size.Height)
        {
        }

        public float Width { get; }

        public float Height { get; }

        public void BeginPath()
        {
            if (_path != null)
                _path.Dispose();
            _path = new GraphicsPath();
            _path.StartFigure();
            _cursor = new PointF(0, 0);
        }

        public void ClosePath()
            => _path.CloseFigure();

        public void MoveTo(float x, float y)
            => _cursor = new PointF(x, y);

        public void LineTo(float x, float y)
        {
            var point = new PointF(x, y);
            _path.AddLine(_cursor, point);
            _cursor = point;
        }
        public void Arc(float x, float y, float radius, float start, float end)
            => _path.AddArc(new RectangleF((x - radius), (y - radius), radius * 2.0f, radius * 2.0f), (start * 180 / (float)Math.PI), ((end - start) * 180 / (float)Math.PI));

        public void Elipse(float x, float y, float rx, float ry, float start, float end)
            => _path.AddArc(new RectangleF((x - rx), (y - ry), rx * 2.0f, ry * 2.0f), (start * 180 / (float)Math.PI), ((end - start) * 180 / (float)Math.PI));

        public void QuadTo(float x1, float y1, float x, float y)
        {
            var point1 = new PointF(x1, y1);
            var pointE = new PointF(x, y);
            _path.AddCurve(new [] { _cursor, point1, pointE });
            _cursor = pointE;
        }
        public void CurveTo(float x1, float y1, float x2, float y2, float x, float y)
        {
            var point1 = new PointF(x1, y1);
            var point2 = new PointF(x2, y2);
            var pointE = new PointF(x, y);
            _path.AddBeziers(new [] { _cursor, point1, point2, pointE });
            _cursor = pointE;
        }

        public void Rect(float x, float y, float width, float height, float rx = 0, float ry = 0)
        {
            var T = 1.0f - 0.707f;
            rx = Math.Max(0, Math.Min(rx, width / 2));
            ry = Math.Max(0, Math.Min(ry, height / 2));
            bool rounded = rx != 0 && ry != 0;
            bool useElipse = true;
            BeginPath();
            MoveTo(x + rx, y);
            LineTo(x + width - rx, y);
            if (rounded && useElipse)
                Elipse(x + width - rx, y + ry, rx, ry, -(float)Math.PI / 2, 0);
            else if (rounded)
                CurveTo(x + width - rx * T, y, x + width, y + ry * T, x + width, y + ry);
            LineTo(x + width, y + height - ry);
            if (rounded)
                CurveTo(x + width, y + height - ry * T, x + width - rx * T, y + height, x + width - rx, y + height);
            LineTo(x + rx, y + height);
            if (rounded)
                CurveTo(x + rx * T, y + height, x, y + height - ry * T, x, y + height - ry);
            LineTo(x, y + ry);
            if (rounded)
                CurveTo(x, y + ry * T, x + rx * T, y, x + rx, y);
            ClosePath();
            // _path.AddRectangle(new RectangleF(x, y, width, height));
        }

        private Color _fillColor;
        private Color _strokeColor;
        private float _strokeWidth;

        public void Fill()
        {
            using var brush = new SolidBrush(_fillColor);
            _graphics.FillPath(brush, _path);
        }

        public void FillStyle(Color color)
        {
            _fillColor = color;
        }

        public void Stroke()
        {
            using var pen = new Pen(_strokeColor, _strokeWidth);
            _graphics.DrawPath(pen, _path);
        }

        public void StrokeStyle(Color color, float width = 1.0f)
        {
            _strokeColor = color;
            _strokeWidth = width;
        }

        TextVariant _fontVariant;
        public void FontStyle(string family, float size, TextVariant variant = TextVariant.None)
        {
            _fontFamily = family;
            _fontSize = size;
            _fontVariant = variant;
        }
        public void Text(float x, float y, string value, TextAlignement align = TextAlignement.Left)
        {
            using var brush = new SolidBrush(_fillColor);
            var style = System.Drawing.FontStyle.Regular;
            if (_fontVariant.HasFlag(TextVariant.Bold))
                style |= System.Drawing.FontStyle.Bold;
            if (_fontVariant.HasFlag(TextVariant.Italic))
                style |= System.Drawing.FontStyle.Italic;
            if (_fontVariant.HasFlag(TextVariant.Underline))
                style |= System.Drawing.FontStyle.Underline;
            if (_fontVariant.HasFlag(TextVariant.Strikeout))
                style |= System.Drawing.FontStyle.Strikeout;
            using var font = new System.Drawing.Font(_fontFamily, _fontSize * 0.65f, style);
            var point = new PointF(x, y);

            var msr = _graphics.MeasureString(value, font);
            if (align.HasFlag(TextAlignement.Right))
                point.X -= msr.Width;
            else if (align.HasFlag(TextAlignement.Center))
                point.X -= msr.Width / 2.0f;
            if (align.HasFlag(TextAlignement.Bottom))
                point.Y -= msr.Height;
            else if (align.HasFlag(TextAlignement.Middle))
                point.Y -= msr.Height / 2.0f;

            _graphics.DrawString(value, font, brush, point);
        }


        public void Restore()
            => _graphics.Restore(_saveStack.Pop());

        public void Save()
            => _saveStack.Push(_graphics.Save());


        public void Rotate(float arg)
            => _graphics.RotateTransform((arg * 180 / (float)Math.PI));

        public void Scale(float x, float y)
            => _graphics.ScaleTransform(x, y);

        public void Translate(float x, float y)
            => _graphics.TranslateTransform(x, y);

        public void DrawImage(float x, float y, Bitmap img)
        {
            var sc = 50.0f;
            _graphics.ScaleTransform(1f/sc, 1f/ sc);
            _graphics.DrawImage(img, (int)(x * sc), (int)(y * sc));
            _graphics.ScaleTransform(sc, sc);
        }

        public FontMeasure MeasureText(string text)
        {
            var style = System.Drawing.FontStyle.Regular;
            if (_fontVariant.HasFlag(TextVariant.Bold))
                style |= System.Drawing.FontStyle.Bold;
            if (_fontVariant.HasFlag(TextVariant.Italic))
                style |= System.Drawing.FontStyle.Italic;
            if (_fontVariant.HasFlag(TextVariant.Underline))
                style |= System.Drawing.FontStyle.Underline;
            if (_fontVariant.HasFlag(TextVariant.Strikeout))
                style |= System.Drawing.FontStyle.Strikeout;
            using var font = new System.Drawing.Font(_fontFamily, _fontSize * 0.65f, style);

            var asize = _graphics.MeasureString("A", font);
            var psize = _graphics.MeasureString("p", font);
            var fsize = _graphics.MeasureString("A|p", font);
            var size = _graphics.MeasureString(text, font);

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