using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XText : IShape
    {
        public IStyle Style { get; set; }
        public string Text { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFilled { get; set; }
        public HAlignment HAlignment { get; set; }
        public VAlignment VAlignment { get; set; }
        public double FontSize { get; set; }
        public string FontName { get; set; }

        public void Render(object dc, IRenderer renderer, IStyle style)
        {
            renderer.DrawText(dc, style, this);
        }
    }
}
