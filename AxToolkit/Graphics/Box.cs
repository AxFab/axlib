using System.Drawing;

namespace AxToolkit.Graphics
{
    public struct Box
    {
        public Box(float value)
        {
            Left = value;
            Right = value;
            Top = value;
            Bottom = value;
        }
        public Box(float left, float top, float right, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
        public Box(float left, float top, SizeF size)
        {
            Left = left;
            Right = left + size.Width;
            Top = top;
            Bottom = top + size.Height;
        }

        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Width => Right - Left;
        public float Height => Bottom - Top;
        public float SumWidth => Right + Left;
        public float SumHeight => Bottom + Top;

        public Box Srink(Box value)
        {
            return new Box
            {
                Left = Left + value.Left,
                Right = Right - value.Right,
                Top = Top + value.Top,
                Bottom = Bottom - value.Bottom,
            };
        }
        public Box Expand(Box value)
        {
            return new Box
            {
                Left = Left - value.Left,
                Right = Right + value.Right,
                Top = Top - value.Top,
                Bottom = Bottom + value.Bottom,
            };
        }
    }
}
