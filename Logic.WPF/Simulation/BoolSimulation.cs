// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation
{
    public abstract class BoolSimulation
    {
        public abstract string Key { get; }
        public abstract Func<XBlock, BoolSimulation> Factory { get; }
        public BoolInput[] Inputs { get; set; }
        public bool? State { get; set; }
        public abstract void Run(IClock clock);
    }
}
