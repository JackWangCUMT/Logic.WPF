using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public interface IColor
    {
        int A { get; set; }
        int R { get; set; }
        int G { get; set; }
        int B { get; set; }
    }
}
