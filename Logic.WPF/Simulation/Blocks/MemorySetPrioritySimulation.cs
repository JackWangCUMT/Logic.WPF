using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class MemorySetPrioritySimulation : BoolSimulation
    {
        public override string Key
        {
            get { return "SR-SET"; }
        }

        public override Func<XBlock, BoolSimulation> Factory
        {
            get { return (block) => { return new MemorySetPrioritySimulation(); }; }
        }

        public override void Run(IClock clock)
        {
            // TODO: Implement simulation
        }
    }
}
