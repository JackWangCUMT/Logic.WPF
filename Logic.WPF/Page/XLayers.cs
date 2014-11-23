using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Page
{
    public class XLayers
    {
        public XLayer Shapes { get; set; }
        public XLayer Blocks { get; set; }
        public XLayer Wires { get; set; }
        public XLayer Pins { get; set; }
        public XLayer Editor { get; set; }
        public XLayer Overlay { get; set; }

        public XPage ToPage(string name, ITemplate template)
        {
            return new XPage()
            {
                Name = name,
                Shapes = Shapes.Shapes,
                Blocks = Blocks.Shapes,
                Pins = Pins.Shapes,
                Wires = Wires.Shapes,
                Template = template
            };
        }
    }
}
