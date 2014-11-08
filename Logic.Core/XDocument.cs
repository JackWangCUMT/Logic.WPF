using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XDocument
    {
        public string Name { get; set; }
        public IList<XPage> Pages { get; set; }
    }
}
