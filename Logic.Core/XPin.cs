using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XPin : IShape
    {
        public IStyle Style { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public PinType PinType { get; set; }
        public XBlock Owner { get; set; }

        public void Render(object dc, IRenderer renderer, IStyle style)
        {
            renderer.DrawPin(dc, style, this);
        }
    }
}
