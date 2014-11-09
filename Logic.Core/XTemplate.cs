using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XTemplate
    {
        public string Name { get; set; }
        public IList<IStyle> Styles { get; set; }
        public XContainer Grid { get; set; }
        public XContainer Table { get; set; }
        public XContainer Frame { get; set; }
    }
}
