using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class AndSimulation : BoolSimulation
    {
        public override void Run()
        {
            int length = Inputs.Length;
            if (length == 1)
            {
                State = null;
                return;
            }

            bool? result = null;
            for (int i = 0; i < length; i++)
            {
                var input = Inputs[i];
                if (i == 0)
                {
                    result = input.IsInverted ? !(input.Simulation.State) : input.Simulation.State;
                }
                else
                {
                    result &= input.IsInverted ? !(input.Simulation.State) : input.Simulation.State;
                }
            }
            State = result;
        }
    }
}
