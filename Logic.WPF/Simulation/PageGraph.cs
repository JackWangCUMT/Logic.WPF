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
    public static class PageGraph
    {
        public static PageGraphContext Create(IPage page)
        {
            return Create(
                page.Blocks.Cast<XBlock>(),
                page.Pins.Cast<XPin>(),
                page.Wires.Cast<XWire>());
        }

        public static PageGraphContext Create(
            IEnumerable<XBlock> blocks,
            IEnumerable<XPin> pins,
            IEnumerable<XWire> wires)
        {
            var context = new PageGraphContext();
            context.Connections = FindConnections(blocks, pins, wires);
            context.Dependencies = FindDependencies(blocks, context.Connections);
            context.PinTypes = FindPinTypes(blocks, pins, context.Dependencies);
            context.OrderedBlocks = SortDependencies(blocks, context.Dependencies, context.PinTypes);

            return context;
        }

        public static IDictionary<XPin, ICollection<Tuple<XPin, bool>>> FindConnections(
            IEnumerable<XBlock> blocks,
            IEnumerable<XPin> pins,
            IEnumerable<XWire> wires)
        {
            var connections = new Dictionary<XPin, ICollection<Tuple<XPin, bool>>>();

            foreach (var block in blocks)
            {
                foreach (var pin in block.Pins.Cast<XPin>())
                {
                    connections.Add(pin, new HashSet<Tuple<XPin, bool>>());
                }
            }

            foreach (var pin in pins)
            {
                connections.Add(pin, new HashSet<Tuple<XPin, bool>>());
            }

            foreach (var wire in wires)
            {
                var startConnections = connections[wire.Start];
                var endConnections = connections[wire.End];
                bool isPinInverted = wire.InvertStart | wire.InvertEnd;

                var et = Tuple.Create(wire.End, isPinInverted);
                if (!startConnections.Contains(et))
                {
                    startConnections.Add(et);
                }
                var st = Tuple.Create(wire.Start, isPinInverted);
                if (!endConnections.Contains(st))
                {
                    endConnections.Add(st);
                }
            }

            return connections;
        }

        public static IDictionary<XPin, ICollection<Tuple<XPin, bool>>> FindDependencies(
            IEnumerable<XBlock> blocks,
            IDictionary<XPin, ICollection<Tuple<XPin, bool>>> connections)
        {
            var dependencies = new Dictionary<XPin, ICollection<Tuple<XPin, bool>>>();

            foreach (var block in blocks)
            {
                foreach (var pin in block.Pins)
                {
                    dependencies.Add(pin, new HashSet<Tuple<XPin, bool>>());
                    FindDependencies(pin, pin, connections, dependencies);
                }
            }

            return dependencies;
        }

        public static void FindDependencies(
            XPin next,
            XPin start,
            IDictionary<XPin, ICollection<Tuple<XPin, bool>>> connections,
            IDictionary<XPin, ICollection<Tuple<XPin, bool>>> dependencies)
        {
            var pinConnections = connections[next];
            foreach (var connection in pinConnections)
            {
                if (connection.Item1 == start)
                {
                    continue;
                }

                var pinDependencies = dependencies[start];
                if (!pinDependencies.Contains(connection))
                {
                    switch (connection.Item1.PinType)
                    {
                        case PinType.None:
                            pinDependencies.Add(connection);
                            break;
                        case PinType.Input:
                            pinDependencies.Add(connection);
                            break;
                        case PinType.Output:
                            pinDependencies.Add(connection);
                            break;
                        case PinType.Standalone:
                            pinDependencies.Add(connection);
                            FindDependencies(connection.Item1, start, connections, dependencies);
                            break;
                    }
                }
            }
        }

        public static IDictionary<XPin, PinType> FindPinTypes(
            IEnumerable<XBlock> blocks,
            IEnumerable<XPin> pins,
            IDictionary<XPin, ICollection<Tuple<XPin, bool>>> dependencies)
        {
            var pinTypes = new Dictionary<XPin, PinType>();
            var pinsWithoutType = new List<XPin>();

            // use pin dependencies to set pin type to Input or Output
            foreach (var block in blocks)
            {
                foreach (var pin in block.Pins)
                {
                    bool haveKey = pinTypes.ContainsKey(pin);
                    if (!haveKey && pin.PinType == PinType.None)
                    {
                        if (dependencies[pin].Count <= 0)
                        {
                            // nothing is connected
                            pinTypes.Add(pin, PinType.None);
                        }
                        else
                        {
                            var pinDependencies = dependencies[pin];
                            int noneDepCount = pinDependencies.Count(p => p.Item1.PinType == PinType.None);
                            int inputDepCount = pinDependencies.Count(p => p.Item1.PinType == PinType.Input);
                            int outputDepCount = pinDependencies.Count(p => p.Item1.PinType == PinType.Output);
                            if (inputDepCount == 0 && outputDepCount > 0 && noneDepCount == 0)
                            {
                                // set as Input
                                pinTypes.Add(pin, PinType.Input);
                            }
                            else if (inputDepCount > 0 && outputDepCount == 0 && noneDepCount == 0)
                            {
                                // set as Output
                                pinTypes.Add(pin, PinType.Output);

                                foreach (var p in pin.Owner.Pins.Where(p => p != pin && dependencies[p].Count > 0))
                                {
                                    if (pinTypes.ContainsKey(p))
                                    {
                                        if (pinTypes[p] == PinType.None)
                                        {
                                            // set as Input
                                            pinTypes[p] = PinType.Input;
                                        }
                                    }
                                    else
                                    {
                                        // set as Input
                                        pinTypes.Add(p, PinType.Input);
                                    }
                                }
                            } 
                            else if (inputDepCount > 0 && outputDepCount > 0)
                            {
                                // invalid pin connection
                                throw new Exception("Conneting Inputs and Outputs to same Pin is not allowed.");
                            }
                            else
                            {
                                // if no Input or Output is connected
                                pinsWithoutType.Add(pin);
                                pinTypes.Add(pin, PinType.None);
                            }
                        }
                    }
                    else
                    {
                        if (!haveKey)
                        {
                            // use pin original type
                            pinTypes.Add(pin, pin.PinType);
                        }
                    }
                }
            }

            if (pinsWithoutType.Count > 0)
            {
                FindPinTypes(dependencies, pinTypes, pinsWithoutType);
            }

            // standalone pins
            foreach (var pin in pins)
            {
                pinTypes.Add(pin, pin.PinType);
            }

            return pinTypes;
        }

        private static void FindPinTypes(
            IDictionary<XPin, ICollection<Tuple<XPin, bool>>> dependencies, 
            Dictionary<XPin, PinType> pinTypes, 
            List<XPin> pins)
        {
            var pinsWithoutType = new List<XPin>();

            // pins with onnections but do not have Input or Output type set
            foreach (var pin in pins)
            {
                XBlock owner = pin.Owner;

                int noneCount = owner.Pins.Count(p => pinTypes[p] == PinType.None && dependencies[p].Count > 0);
                int inputCount = owner.Pins.Count(p => pinTypes[p] == PinType.Input);
                int outputCount = owner.Pins.Count(p => pinTypes[p] == PinType.Output);

                var pinDependencies = dependencies[pin];
                int noneDepCount = pinDependencies.Count(
                    p => pinTypes.ContainsKey(p.Item1) && pinTypes[p.Item1] == PinType.None);
                int inputDepCount = pinDependencies.Count(
                    p => pinTypes.ContainsKey(p.Item1) && pinTypes[p.Item1] == PinType.Input);
                int outputDepCount = pinDependencies.Count(
                    p => pinTypes.ContainsKey(p.Item1) && pinTypes[p.Item1] == PinType.Output);

                if (inputDepCount == 0 && outputDepCount > 0 && noneDepCount == 0)
                {
                    // set as Input
                    pinTypes[pin] = PinType.Input;
                }
                else if (inputDepCount > 0 && outputDepCount == 0 && noneDepCount == 0)
                {
                    // set as Output
                    pinTypes[pin] = PinType.Output;

                    foreach (var p in pin.Owner.Pins.Where(p => p != pin && dependencies[p].Count > 0))
                    {
                        if (pinTypes.ContainsKey(p))
                        {
                            if (pinTypes[p] == PinType.None)
                            {
                                // set as Input
                                pinTypes[p] = PinType.Input;
                            }
                        }
                        else
                        {
                            // set as Input
                            pinTypes.Add(p, PinType.Input);
                        }
                    }
                }
                else
                {
                    if (pinTypes[pin] == PinType.None)
                    {
                        if (noneCount == 1 && inputCount == 1 && outputCount == 0)
                        {
                            // set as Output
                            pinTypes[pin] = PinType.Output;
                        }
                        else
                        {
                            pinsWithoutType.Add(pin);
                        }
                    }
                }
            }

            if (pinsWithoutType.Count > 0 && pins.Count == pinsWithoutType.Count)
            {
                throw new Exception("Can not find pin types.");
            }

            if (pinsWithoutType.Count > 0)
            {
                FindPinTypes(dependencies, pinTypes, pinsWithoutType);
            }
        }

        public static IList<XBlock> SortDependencies(
            IEnumerable<XBlock> blocks,
            IDictionary<XPin, ICollection<Tuple<XPin, bool>>> dependencies,
            IDictionary<XPin, PinType> pinTypes)
        {
            var dict = new Dictionary<XBlock, IList<XBlock>>();

            foreach (var block in blocks)
            {
                dict.Add(block, new List<XBlock>());

                foreach (var pin in block.Pins)
                {
                    var pinDependencies = dependencies[pin]
                        .Where(p => pinTypes[p.Item1] == PinType.Input);

                    foreach (var dependency in pinDependencies)
                    {
                        dict[block].Add(dependency.Item1.Owner);
                    }
                }
            }

            // sort blocks using Pins dependencies
            var tsort = new TopologicalSort<XBlock>();
            var sorted = tsort.Sort(
                blocks, 
                block => dict[block], 
                false);

            return sorted.Reverse().ToList();
        }
    }
}
