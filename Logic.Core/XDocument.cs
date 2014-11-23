using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XDocument : IDocument
    {
        public string Name { get; set; }
        public IList<IPage> Pages { get; set; }
    }
}
