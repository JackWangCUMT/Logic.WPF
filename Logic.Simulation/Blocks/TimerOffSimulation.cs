using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class TimerOffSimulation : BoolSimulation
    {
        public double Delay { get; set; }

        public TimerOffSimulation()
            : base()
        {
        }

        public TimerOffSimulation(double delay)
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
