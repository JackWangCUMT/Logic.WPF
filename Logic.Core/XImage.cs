using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XImage : IShape
    {
        public IProperty[] Properties { get; set; }
        public IStyle Style { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Uri Path { get; set; }

        public void Render(object dc, IRenderer renderer, IStyle style)
        {
            renderer.DrawImage(dc, style, this);
        }
    }
}
