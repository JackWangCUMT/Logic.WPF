// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Simulation
{
    public class PageGraphContext
    {
        public IDictionary<XPin, ICollection<Tuple<XPin, bool>>> Connections { get; set; }
        public IDictionary<XPin, ICollection<Tuple<XPin, bool>>> Dependencies { get; set; }
        public IDictionary<XPin, PinType> PinTypes { get; set; }
        public IList<XBlock> OrderedBlocks { get; set; }
    }
}
