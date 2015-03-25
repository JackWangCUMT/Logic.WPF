// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class OrSimulation : BoolSimulation
    {
        public override string Key
        {
            get { return "OR"; }
        }

        public override Func<XBlock, BoolSimulation> Factory
        {
            get { return (block) => { return new OrSimulation(null, block.GetIntPropertyValue("Counter")); }; }
        }

        public int Counter { get; set; }

        public OrSimulation()
            : base()
        {
        }

        public OrSimulation(bool? state, int counter)
            : base()
        {
            base.State = state;
            this.Counter = counter;
        }

        public override void Run(IClock clock)
        {
            int length = Inputs.Length;
            if (length == 1)
            {
                base.State = null;
                return;
            }

            if (Counter <= 0)
            {
                throw new Exception("Or counter must greater than or equal 1.");
            }
            else
            {
                int counter = 0;
                bool? result = null;
                for (int i = 0; i < length; i++)
                {
                    var input = Inputs[i];
                    if (i == 0)
                    {
                        result = input.IsInverted ? !(input.Simulation.State) : input.Simulation.State;
                        if (result == true)
                        {
                            counter += 1;
                        }
                    }
                    else
                    {
                        bool? value = input.IsInverted ? !(input.Simulation.State) : input.Simulation.State;
                        result |= value;
                        if (value == true)
                        {
                            counter += 1;
                        }
                    }
                }

                if (counter >= Counter)
                {
                    base.State = true;
                }
                else if (result == true)
                {
                    base.State = false;
                }
                else
                {
                    base.State = result;
                }
            }
        }
    }
}
