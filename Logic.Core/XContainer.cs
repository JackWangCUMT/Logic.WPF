using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public class XContainer
    {
        public IList<IStyle> Styles { get; set; }
        public IList<IShape> Shapes { get; set; }
    }
}
