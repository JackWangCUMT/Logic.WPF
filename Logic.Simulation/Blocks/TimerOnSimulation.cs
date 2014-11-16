using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class TimerOnSimulation : BoolSimulation
    {
        public double Delay { get; set; }

        public TimerOnSimulation()
            : base()
        {
        }

        public TimerOnSimulation(double delay)
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
