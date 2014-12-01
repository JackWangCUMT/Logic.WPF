using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IPage
    {
        string Name { get; set; }
        bool IsActive { get; set; }
        ITemplate Template { get; set; }
        IList<IShape> Shapes { get; set; }
        IList<IShape> Blocks { get; set; }
        IList<IShape> Pins { get; set; }
        IList<IShape> Wires { get; set; }
    }
}
