using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XProject
    {
        public string Name { get; set; }
        public IList<XTemplate> Templates { get; set; }
        public IList<XDocument> Documents { get; set; }
    }
}
