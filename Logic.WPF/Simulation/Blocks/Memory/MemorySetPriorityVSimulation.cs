// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class MemorySetPriorityVSimulation : BoolSimulation
    {
        public override string Key
        {
            get { return "SR-SET-V"; }
        }

        public override Func<XBlock, BoolSimulation> Factory
        {
            get { return (block) => { return new MemorySetPriorityVSimulation(); }; }
        }

        public override void Run(IClock clock)
        {
            // TODO: Implement simulation
        }
    }
}
