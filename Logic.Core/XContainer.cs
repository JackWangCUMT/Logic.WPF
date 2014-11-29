using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XContainer : IContainer
    {
        public IList<IStyle> Styles { get; set; }
        public IList<IShape> Shapes { get; set; }
    }
}
