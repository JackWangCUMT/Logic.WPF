using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public enum MemoryPriority { Set, Reset }

    public class MemorySetResetSimulation : BoolSimulation
    {
        public MemoryPriority Priority { get; private set; }

        public MemorySetResetSimulation(MemoryPriority priority)
            : base()
        {
            this.Priority = priority;
        }

        public override void Run(IClock clock)
        {
            // TODO: Implement simulation.
        }
    }
}
