using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XBlock : IShape
    {
        public IProperty[] Properties { get; set; }
        public IList<KeyValuePair<string, IProperty>> Database { get; set; }
        public IStyle Style { get; set; }
        public string Name { get; set; }
        public IList<IShape> Shapes { get; set; }
        public IList<XPin> Pins { get; set; }

        public void Render(object dc, IRenderer renderer, IStyle style)
        {
            foreach (var shape in Shapes)
            {
                shape.Render(dc, renderer, style);
            }
        }
    }
}
