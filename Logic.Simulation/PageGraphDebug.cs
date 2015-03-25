// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Simulation
{
    public static class PageGraphDebug
    {
        public static void WriteConnections(PageGraphContext context, TextWriter writer)
        {
            writer.WriteLine("Connections: ");
            foreach (var kvp in context.Connections)
            {
                var pin = kvp.Key;
                var connections = kvp.Value;
                writer.WriteLine(
                    "{0}:{1}",
                    pin.Owner == null ? "<>" : pin.Owner.Name,
                    pin.Name);
                if (connections != null && connections.Count > 0)
                {
                    foreach (var connection in connections)
                    {
                        writer.WriteLine(
                            "\t{0}:{1}, inverted: {2}",
                            connection.Item1.Owner == null ? "<>" : connection.Item1.Owner.Name,
                            connection.Item1.Name,
                            connection.Item2);
                    }
                }
                else
                {
                    writer.WriteLine("\t<none>");
                }
            }
        }

        public static void WriteDependencies(PageGraphContext context, TextWriter writer)
        {
            writer.WriteLine("Dependencies: ");
            foreach (var kvp in context.Dependencies)
            {
                var pin = kvp.Key;
                var dependencies = kvp.Value;
                if (dependencies != null && dependencies.Count > 0)
                {
                    writer.WriteLine(
                        "{0}:{1}",
                        pin.Owner == null ? "<>" : pin.Owner.Name,
                        pin.Name);
                    foreach (var dependency in dependencies)
                    {
                        writer.WriteLine(
                            "\t[{0}] {1}:{2}, inverted: {3}",
                            dependency.Item1.PinType,
                            dependency.Item1.Owner == null ? "<>" : dependency.Item1.Owner.Name,
                            dependency.Item1.Name,
                            dependency.Item2);
                    }
                }
                else
                {
                    writer.WriteLine(
                        "{0}:{1}",
                        pin.Owner == null ? "<>" : pin.Owner.Name,
                        pin.Name);
                    writer.WriteLine("\t<none>");
                }
            }
        }

        public static void WritePinTypes(PageGraphContext context, TextWriter writer)
        {
            writer.WriteLine("PinTypes: ");
            foreach (var kvp in context.PinTypes)
            {
                var pin = kvp.Key;
                var type = kvp.Value;
                writer.WriteLine(
                    "\t[{0}] {1}:{2}",
                    type,
                    pin.Owner == null ? "<>" : pin.Owner.Name,
                    pin.Name);
            }
        }

        public static void WriteOrderedBlocks(PageGraphContext context, TextWriter writer)
        {
            writer.WriteLine("OrderedBlocks: ");
            foreach (var block in context.OrderedBlocks)
            {
                writer.WriteLine(block.Name);
            }
        }
    }
}
