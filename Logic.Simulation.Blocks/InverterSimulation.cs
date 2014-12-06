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
            int length = Inputs.Length;
            if (length == 0)
            {
                // Do nothing.
            }
            else if (length == 1)
            {
                var input = Inputs[0];
                base.State = input.IsInverted ? input.Simulation.State : !(input.Simulation.State);
            }
            else
            {
                throw new Exception("Inverter simulation can only have one input State.");
            }
        }
    }
}
