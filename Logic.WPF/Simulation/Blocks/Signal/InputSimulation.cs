// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class InputSimulation : BoolSimulation
    {
        public override string Key
        {
            get { return "INPUT"; }
        }

        public override Func<XBlock, BoolSimulation> Factory
        {
            get { return (block) => { return new InputSimulation(false); }; }
        }

        public InputSimulation()
            : base()
        {
        }

        public InputSimulation(bool? state)
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
            else
            {
                throw new Exception("Input simulation can not have any inputs connected.");
            }
        }
    }
}
