// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public static class GridFactory
    {
        public class GridSettings
        {
            public double StartX { get; set; }
            public double StartY { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public double SizeX { get; set; }
            public double SizeY { get; set; }
        }

        public static void Create(IList<IShape> shapes, IStyle style, GridSettings settings)
        {
            double sx = settings.StartX + settings.SizeX;
            double sy = settings.StartY + settings.SizeY;
            double ex = settings.StartX + settings.Width;
            double ey = settings.StartY + settings.Height;

            for (double x = sx; x < ex; x += settings.SizeX)
            {
                shapes.Add(new XLine()
                {
                    X1 = x,
                    Y1 = settings.StartY,
                    X2 = x,
                    Y2 = ey,
                    Style = style
                });
            }

            for (double y = sy; y < ey; y += settings.SizeY)
            {
                shapes.Add(new XLine()
                {
                    X1 = settings.StartX,
                    Y1 = y,
                    X2 = ex,
                    Y2 = y,
                    Style = style
                });
            }
        }
    }
}
