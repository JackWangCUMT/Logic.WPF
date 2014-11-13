using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class SignalSimulation : BoolSimulation
    {
        public override void Run()
        {
            int length = Inputs.Length;
            if (length == 0)
            {
                // Do nothing.
            }
            else if (length == 1)
            {
                var input = Inputs[0];
                State = input.IsInverted ? !(input.Simulation.State) : input.Simulation.State;
            }
            else
            {
                throw new Exception("Signal simulation can only have one input State.");
            }
        }
    }
}
