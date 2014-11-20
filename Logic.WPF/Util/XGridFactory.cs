using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Util
{
    public static class XGridFactory
    {
        public class Options
        {
            public double StartX { get; set; }
            public double StartY { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public double SizeX { get; set; }
            public double SizeY { get; set; }
        }

        public static void Create(IList<IShape> shapes, IStyle style, Options options)
        {
            double sx = options.StartX + options.SizeX;
            double sy = options.StartY + options.SizeY;
            double ex = options.StartX + options.Width;
            double ey = options.StartY + options.Height;

            for (double x = sx; x < ex; x += options.SizeX)
            {
                shapes.Add(new XLine()
                {
                    X1 = x,
                    Y1 = options.StartY,
                    X2 = x,
                    Y2 = ey,
                    Style = style
                });
            }

            for (double y = sy; y < ey; y += options.SizeY)
            {
                shapes.Add(new XLine()
                {
                    X1 = options.StartX,
                    Y1 = y,
                    X2 = ex,
                    Y2 = y,
                    Style = style
                });
            }
        }
    }
}
