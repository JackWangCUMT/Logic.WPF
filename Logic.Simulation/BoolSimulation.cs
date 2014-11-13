using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation
{
    public abstract class BoolSimulation
    {
        public BoolInput[] Inputs { get; set; }
        public bool? State { get; set; }
        public abstract void Run();
    }
}
