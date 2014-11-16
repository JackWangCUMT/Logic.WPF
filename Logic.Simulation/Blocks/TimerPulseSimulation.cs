using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class TimerPulseSimulation : BoolSimulation
    {
        public double Delay { get; set; }

        public TimerPulseSimulation()
            : base()
        {
        }

        public TimerPulseSimulation(bool? state, double delay)
            : base()
        {
            base.State = state;
            this.Delay = delay;
        }

        public override void Run(IClock clock)
        {
            // TODO: Implement simulation.
        }
    }
}
