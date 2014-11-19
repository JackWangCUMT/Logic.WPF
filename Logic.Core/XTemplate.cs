using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XTemplate : ITemplate
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public XContainer Grid { get; set; }
        public XContainer Table { get; set; }
        public XContainer Frame { get; set; }
    }
}
