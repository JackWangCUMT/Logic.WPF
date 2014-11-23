using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XPage : IPage
    {
        public string Name { get; set; }
        public ITemplate Template { get; set; }
        public IList<IShape> Shapes { get; set; }
        public IList<IShape> Blocks { get; set; }
        public IList<IShape> Pins { get; set; }
        public IList<IShape> Wires { get; set; }
    }
}
