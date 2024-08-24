using AxToolkit.Graphics;

namespace AxMaui
{
    public class DrawableScreen : IDrawable
    {
        public DrawableScreen(IScreen screen = null) {
            Screen = screen;
        }
        public IScreen Screen { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (dirtyRect.Width == 0 || dirtyRect.Height == 0)
                return;

            if (Screen != null)
            {
                var ctx = new MauiCanvasDrawer(canvas, dirtyRect);
                Screen.Paint(ctx);
            }
            else
            {
                // If not draw a clock
                float radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 3.0f;
                var color = Color.FromArgb("#a61010");
                canvas.StrokeColor = color;
                canvas.StrokeSize = 12;
                canvas.DrawCircle(dirtyRect.Center, radius);
                canvas.DrawCircle(dirtyRect.Center, 3.0f);

                var now = DateTime.Now;
                canvas.StrokeSize = 1;
                var min = now.Second + now.Millisecond / 1000.0;
                DrawP(canvas, dirtyRect, radius * 0.85f, (float)min, 60.0f);

                canvas.StrokeSize = 4;
                var hour = now.Minute + now.Second / 60.0 + now.Millisecond / 60000.0;
                DrawP(canvas, dirtyRect, radius * 0.80f, (float)hour, 60.0f);

                var half = (now.Hour % 12) + now.Minute / 60.0 + now.Second / 3600.0 + now.Millisecond / 3600000.0;
                DrawP(canvas, dirtyRect, radius * 0.60f, (float)half, 12.0f);
            }
        }

        private void DrawP(ICanvas canvas, RectF dirtyRect, float radius, float value, float maximum)
        {
            var angle = 2.0 * Math.PI * value / maximum;
            var pt = new PointF
            {
                X = dirtyRect.Center.X + radius * (float)Math.Sin(angle),
                Y = dirtyRect.Center.Y - radius * (float)Math.Cos(angle),
            };
            canvas.DrawLine(dirtyRect.Center, pt);
        }

    }
}
