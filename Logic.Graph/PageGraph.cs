using Logic.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Graph
{
    public static class PageGraph
    {
        public static PageGraphContext Create(XPage page)
        {
            var blocks = page.Blocks.Cast<XBlock>();
            var pins = page.Pins.Cast<XPin>();
            var wires = page.Wires.Cast<XWire>();

            var context = new PageGraphContext();

            context.Connections = FindConnections(blocks, pins, wires);
            context.Dependencies = FindDependencies(blocks, context.Connections);
            context.PinTypes = FindPinTypes(blocks, pins, context.Dependencies);
            context.OrderedBlocks = SortDependencies(blocks, context.Dependencies, context.PinTypes);

            return context;
        }

        public static IDictionary<XPin, ICollection<XPin>> FindConnections(
            IEnumerable<XBlock> blocks,
            IEnumerable<XPin> pins,
            IEnumerable<XWire> wires)
        {
            var connections = new Dictionary<XPin, ICollection<XPin>>();

            foreach (var block in blocks)
            {
                foreach (var pin in block.Pins.Cast<XPin>())
                {
                    connections.Add(pin, new HashSet<XPin>());
                }
            }

            foreach (var pin in pins)
            {
                connections.Add(pin, new HashSet<XPin>());
            }

            foreach (var wire in wires)
            {
                var startConnections = connections[wire.Start];
                var endConnections = connections[wire.End];

                if (!startConnections.Contains(wire.End))
                {
                    startConnections.Add(wire.End);
                }

                if (!endConnections.Contains(wire.Start))
                {
                    endConnections.Add(wire.Start);
                }
            }

            return connections;
        }

        public static IDictionary<XPin, ICollection<XPin>> FindDependencies(
            IEnumerable<XBlock> blocks,
            IDictionary<XPin, ICollection<XPin>> connections)
        {
            var dependencies = new Dictionary<XPin, ICollection<XPin>>();

            foreach (var block in blocks)
            {
                foreach (var pin in block.Pins)
                {
                    dependencies.Add(pin, new HashSet<XPin>());
                    FindDependencies(pin, pin, connections, dependencies);
                }
            }

            return dependencies;
        }

        public static void FindDependencies(
            XPin next,
            XPin start,
            IDictionary<XPin, ICollection<XPin>> connections,
            IDictionary<XPin, ICollection<XPin>> dependencies)
        {
            var pinConnections = connections[next];
            foreach (var connection in pinConnections)
            {
                if (connection == start)
                {
                    continue;
                }

                var pinDependencies = dependencies[start];
                if (!pinDependencies.Contains(connection))
                {
                    switch (connection.PinType)
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
                            FindDependencies(connection, start, connections, dependencies);
                            break;
                    }
                }
            }
        }

        public static IDictionary<XPin, PinType> FindPinTypes(
            IEnumerable<XBlock> blocks,
            IEnumerable<XPin> pins,
            IDictionary<XPin, ICollection<XPin>> dependencies)
        {
            var pinTypes = new Dictionary<XPin, PinType>();

            // using pin dependencies set pins with None type to Input or Output type
            foreach (var block in blocks)
            {
                bool hasInput = false;
                bool hasOutput = false;

                foreach (var pin in block.Pins)
                {
                    if (pin.PinType == PinType.None)
                    {
                        var pinDependencies = dependencies[pin];
                        var noneCount = pinDependencies.Count(p => p.PinType == PinType.None);
                        var inputCount = pinDependencies.Count(p => p.PinType == PinType.Input);
                        var outputCount = pinDependencies.Count(p => p.PinType == PinType.Output);
                        var standaloneCount = pinDependencies.Count(p => p.PinType == PinType.Standalone);
                        // set as Input
                        if (inputCount == 0 && outputCount > 0 && noneCount == 0)
                        {
                            pinTypes.Add(pin, PinType.Input);
                            hasInput = true;
                        }
                        // set as Output
                        else if (inputCount > 0 && outputCount == 0 && noneCount == 0)
                        {
                            pinTypes.Add(pin, PinType.Output);
                            hasOutput = true;
                        }
                        // invalid pin connection
                        else if (inputCount > 0 && outputCount > 0)
                        {
                            throw new Exception("Conneting Inputs and Outputs to same Pin is not allowed.");
                        }
                        // if no Input or Output is connected
                        else
                        {
                            // already have one Input, set pin as Output
                            if (hasInput && !hasOutput)
                            {
                                pinTypes.Add(pin, PinType.Output);
                                hasOutput = true;
                            }
                            // already have one Output, set pin as Input
                            else if (!hasInput && hasOutput)
                            {
                                pinTypes.Add(pin, PinType.Input);
                                hasInput = true;
                            }
                            // assume that pin is Input in onlyne None pins are connected
                            else if (noneCount > 0)
                            {
                                pinTypes.Add(pin, PinType.Input);
                                hasInput = true;
                            }
                            // nothing is connected
                            else
                            {
                                pinTypes.Add(pin, PinType.None);
                            }
                        }
                    }
                    // use pin original type
                    else
                    {
                        pinTypes.Add(pin, pin.PinType);
                    }
                }
            }

            foreach (var pin in pins)
            {
                pinTypes.Add(pin, pin.PinType);
            }

            return pinTypes;
        }

        public static IList<XBlock> SortDependencies(
            IEnumerable<XBlock> blocks,
            IDictionary<XPin, ICollection<XPin>> dependencies,
            IDictionary<XPin, PinType> pinTypes)
        {
            var dict = new Dictionary<XBlock, IList<XBlock>>();

            foreach (var block in blocks)
            {
                dict.Add(block, new List<XBlock>());

                foreach (var pin in block.Pins)
                {
                    var pinDependencies = dependencies[pin]
                        .Where(p => pinTypes[p] == PinType.Input);

                    foreach (var dependency in pinDependencies)
                    {
                        dict[block].Add(dependency.Owner);
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
