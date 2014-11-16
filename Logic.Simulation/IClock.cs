using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation
{
    public interface IClock
    {
        long Cycle { get; }
        int Resolution { get; }
    }
}
