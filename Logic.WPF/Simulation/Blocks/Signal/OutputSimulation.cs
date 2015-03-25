// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class OutputSimulation : BoolSimulation
    {
        public override string Key
        {
            get { return "OUTPUT"; }
        }

        public override Func<XBlock, BoolSimulation> Factory
        {
            get { return (block) => { return new OutputSimulation(false); }; }
        }

        public OutputSimulation()
            : base()
        {
        }

        public OutputSimulation(bool? state)
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
                base.State = input.IsInverted ? !(input.Simulation.State) : input.Simulation.State;
            }
            else
            {
                throw new Exception("Output simulation can only have one input State.");
            }
        }
    }
}
