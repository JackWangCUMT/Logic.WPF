using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XBlock : IShape
    {
        public IStyle Style { get; set; }
        public string Name { get; set; }
        public IList<IShape> Shapes { get; set; }
        public IList<XPin> Pins { get; set; }

        public void Render(object dc, IRenderer renderer)
        {
            renderer.DrawBlock(dc, this);
        }
    }
}
