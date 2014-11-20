using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Page
{
    public class XLayers
    {
        public XCanvas Shapes { get; set; }
        public XCanvas Blocks { get; set; }
        public XCanvas Wires { get; set; }
        public XCanvas Pins { get; set; }
        public XCanvas Overlay { get; set; }
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
