using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XPage
    {
        public string Name { get; set; }
        public XTemplate Template { get; set; }
        public IList<IShape> Shapes { get; set; }
        public IList<IShape> Blocks { get; set; }
        public IList<IShape> Pins { get; set; }
        public IList<IShape> Wires { get; set; }
    }
}
