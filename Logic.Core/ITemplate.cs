using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public interface ITemplate
    {
        string Name { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        XContainer Grid { get; set; }
        XContainer Table { get; set; }
        XContainer Frame { get; set; }
    }
}
