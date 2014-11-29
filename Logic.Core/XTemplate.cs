using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XTemplate : ITemplate
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public IContainer Grid { get; set; }
        public IContainer Table { get; set; }
        public IContainer Frame { get; set; }
    }
}
