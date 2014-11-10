using Logic.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Graph
{
    public class PageGraph
    {
        public IDictionary<XPin, ICollection<XPin>> Connections { get; set; }
        public IDictionary<XPin, ICollection<XPin>> Dependencies { get; set; }
        public IDictionary<XPin, PinType> PinTypes { get; set; }
        public IList<XBlock> OrderedBlocks { get; set; }

        public void Create(XPage page)
        {
            Connections = FindConnections(page);
            Dependencies = FindDependencies(page, Connections);
            PinTypes = FindPinTypes(page, Dependencies);
            OrderedBlocks = SortDependencies(page, Dependencies, PinTypes);
        }

        public IList<XBlock> SortDependencies(
            XPage page,
            IDictionary<XPin, ICollection<XPin>> dependencies,
            IDictionary<XPin, PinType> pinTypes)
        {
            // sort blocks using Pins dependencies
            var dict = new Dictionary<XBlock, IList<XBlock>>();

            foreach (var block in page.Blocks.Cast<XBlock>())
            {
                dict.Add(block, new List<XBlock>());
                foreach (var pin in block.Pins)
                {
                    var pinDependencies = dependencies[pin].Where(p => pinTypes[p] == PinType.Input);
                    foreach (var dependency in pinDependencies)
                        dict[block].Add(dependency.Owner);
                }
            }

            var ts = new TopologicalSort<XBlock>();
            var sorted = ts.Sort(page.Blocks.Cast<XBlock>(), block => dict[block], true);
            return sorted.Reverse().ToList();
        }

        public IDictionary<XPin, PinType> FindPinTypes(
            XPage page,
            IDictionary<XPin, ICollection<XPin>> dependencies)
        {
            var pinTypes = new Dictionary<XPin, PinType>();

            // using pin dependencies set pins with None type to Input or Output type
            foreach (var block in page.Blocks.Cast<XBlock>())
            {
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
                        if (inputCount == 0 && outputCount > 0)
                        {
                            pinTypes.Add(pin, PinType.Input);
                        }
                        // set as Output
                        else if (inputCount > 0 && outputCount == 0)
                        {
                            pinTypes.Add(pin, PinType.Output);
                        }
                        else if (inputCount > 0 && outputCount > 0)
                        {
                            throw new Exception("Conneting Inputs and Outputs to same Pin is not allowed.");
                        }
                    }
                    else
                    {
                        pinTypes.Add(pin, pin.PinType);
                    }
                }
            }

            foreach (var pin in page.Pins.Cast<XPin>())
            {
                pinTypes.Add(pin, pin.PinType);
            }

            return pinTypes;
        }

        public IDictionary<XPin, ICollection<XPin>> FindDependencies(
            XPage page,
            IDictionary<XPin, ICollection<XPin>> connections)
        {
            var dependencies = new Dictionary<XPin, ICollection<XPin>>();

            foreach (var block in page.Blocks.Cast<XBlock>())
            {
                foreach (var pin in block.Pins.Cast<XPin>())
                {
                    dependencies.Add(pin, new HashSet<XPin>());
                    FindDependencies(pin, pin, connections, dependencies);
                }
            }

            return dependencies;
        }

        public void FindDependencies(
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

        public IDictionary<XPin, ICollection<XPin>> FindConnections(XPage page)
        {
            var connections = new Dictionary<XPin, ICollection<XPin>>();

            foreach (var block in page.Blocks.Cast<XBlock>())
            {
                foreach (var pin in block.Pins.Cast<XPin>())
                {
                    connections.Add(pin, new HashSet<XPin>());
                }
            }

            foreach (var pin in page.Pins.Cast<XPin>())
            {
                connections.Add(pin, new HashSet<XPin>());
            }

            foreach (var wire in page.Wires.Cast<XWire>())
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

        #if DEBUG

        public void DisplayConnections()
        {
            Debug.Print("Connections: ");
            foreach (var kvp in Connections)
            {
                var pin = kvp.Key;
                var connections = kvp.Value;
                Debug.Print(
                    "{0}:{1}",
                    pin.Owner == null ? "<>" : pin.Owner.Name,
                    pin.Name);
                if (connections != null && connections.Count > 0)
                {
                    foreach (var connection in connections)
                    {
                        Debug.Print(
                            "\t{0}:{1}",
                            connection.Owner == null ? "<>" : connection.Owner.Name,
                            connection.Name);
                    }
                }
                else
                {
                    Debug.Print("\t<none>");
                }
            }
        }

        public void DisplayDependencies()
        {
            Debug.Print("Dependencies: ");
            foreach (var kvp in Dependencies)
            {
                var pin = kvp.Key;
                var dependencies = kvp.Value;
                if (dependencies != null && dependencies.Count > 0)
                {
                    Debug.Print(
                        "{0}:{1}",
                        pin.Owner == null ? "<>" : pin.Owner.Name,
                        pin.Name);
                    foreach (var dependency in dependencies)
                    {
                        Debug.Print(
                            "\t[{0}] {1}:{2}",
                            dependency.PinType,
                            dependency.Owner == null ? "<>" : dependency.Owner.Name,
                            dependency.Name);
                    }
                }
                else
                {
                    Debug.Print(
                        "{0}:{1}",
                        pin.Owner == null ? "<>" : pin.Owner.Name,
                        pin.Name);
                    Debug.Print("\t<none>");
                }
            }
        }

        public void DisplayPinTypes()
        {
            Debug.Print("PinTypes: ");
            foreach (var kvp in PinTypes)
            {
                var pin = kvp.Key;
                var type = kvp.Value;
                Debug.Print(
                    "\t[{0}] {1}:{2}",
                    type,
                    pin.Owner == null ? "<>" : pin.Owner.Name,
                    pin.Name);
            }
        }

        public void DisplayOrderedBlocks()
        {
            Debug.Print("OrderedBlocks: ");
            foreach (var block in OrderedBlocks)
            {
                Debug.Print(block.Name);
            }
        }

        #endif
    }
}
