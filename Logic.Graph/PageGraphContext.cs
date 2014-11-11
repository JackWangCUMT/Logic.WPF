using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Graph
{
    public class PageGraphContext
    {
        public IDictionary<XPin, ICollection<XPin>> Connections { get; set; }
        public IDictionary<XPin, ICollection<XPin>> Dependencies { get; set; }
        public IDictionary<XPin, PinType> PinTypes { get; set; }
        public IList<XBlock> OrderedBlocks { get; set; }
    }
}
