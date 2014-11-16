using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class InverterSimulation : BoolSimulation
    {
        public InverterSimulation()
            : base()
        {
        }

        public InverterSimulation(bool? state)
            : base()
        {
            base.State = state;
        }

        public override void Run(IClock clock)
        {
            // TODO: Implement simulation.
        }
    }
}
