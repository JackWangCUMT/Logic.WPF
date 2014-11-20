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
        public NativeCanvas Shapes { get; set; }
        public NativeCanvas Blocks { get; set; }
        public NativeCanvas Wires { get; set; }
        public NativeCanvas Pins { get; set; }
        public NativeCanvas Overlay { get; set; }
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
