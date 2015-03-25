// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Simulation
{
    public class BoolSimulationFactory
    {
        public IDictionary<string, Func<XBlock, BoolSimulation>> Registry { get; private set; }

        public BoolSimulationFactory()
        {
            Registry = new Dictionary<string, Func<XBlock, BoolSimulation>>();
        }

        public static T GetInstance<T>() where T: BoolSimulation
        {
            return (T)Activator.CreateInstance(typeof(T), true);
        }

        public bool Register(BoolSimulation simulation)
        {
            if (Registry.ContainsKey(simulation.Key))
            {
                return false;
            }
            Registry.Add(simulation.Key, simulation.Factory);
            return true;
        }

        public void Register(IEnumerable<BoolSimulation> simulations)
        {
            foreach (var simulation in simulations)
            {
                Register(simulation);
            }
        }

        public bool Register(string key, Func<XBlock, BoolSimulation> factory)
        {
            if (Registry.ContainsKey(key))
            {
                return false;
            }
            Registry.Add(key, factory);
            return true;
        }

        private IDictionary<XBlock, BoolSimulation> GetSimulations(PageGraphContext context)
        {
            var simulations = new Dictionary<XBlock, BoolSimulation>();
            foreach (var block in context.OrderedBlocks)
            {
                if (Registry.ContainsKey(block.Name))
                {
                    simulations.Add(block, Registry[block.Name](block));
                }
                else
                {
                    throw new Exception("Not supported block simulation.");
                }
            }
            return simulations;
        }

        public IDictionary<XBlock, BoolSimulation> Create(PageGraphContext context)
        {
            var simulations = GetSimulations(context);

            // find ordered block Inputs
            foreach (var block in context.OrderedBlocks)
            {
                var inputs = block.Pins
                    .Where(pin => context.PinTypes[pin] == PinType.Input)
                    .SelectMany(pin =>
                    {
                        return context.Dependencies[pin]
                            .Where(dep => context.PinTypes[dep.Item1] == PinType.Output);
                    })
                    .Select(pin => pin);

                // convert inputs to BoolInput
                var simulation = simulations[block];
                simulation.Inputs = inputs.Select(input =>
                {
                    return new BoolInput()
                    {
                        Simulation = simulations[input.Item1.Owner],
                        IsInverted = input.Item2
                    };
                }).ToArray();
            }
            return simulations;
        }

        public void Run(IDictionary<XBlock, BoolSimulation> simulations, IClock clock)
        {
            foreach (var simulation in simulations)
            {
                simulation.Value.Run(clock);
            }
        }
    }
}
