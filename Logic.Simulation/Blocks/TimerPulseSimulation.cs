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

        public TimerPulseSimulation(double delay)
            : base()
        {
            this.Delay = delay;
        }

        public override void Run()
        {
            // TODO: Implement simulation.
        }
    }
}
