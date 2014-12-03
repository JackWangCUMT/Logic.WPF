using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface ITemplate
    {
        string Name { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        IContainer Grid { get; set; }
        IContainer Table { get; set; }
        IContainer Frame { get; set; }
    }
}
