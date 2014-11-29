using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IContainer
    {
        IList<IStyle> Styles { get; set; }
        IList<IShape> Shapes { get; set; }
    }
}
