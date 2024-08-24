using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Graphics
{
    public interface IScreen
    {
        void Paint(IDrawingContext ctx);
        void MouseOver(double x, double y);
        void MouseClick(double x, double y, int btn);
    }
}
