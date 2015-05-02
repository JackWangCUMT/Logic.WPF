// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XEllipse : IShape
    {
        public IProperty[] Properties { get; set; }
        public IStyle Style { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
        public bool IsFilled { get; set; }

        public void Render(object dc, IRenderer renderer, IStyle style)
        {
            renderer.DrawEllipse(dc, style, this);
        }
    }
}
