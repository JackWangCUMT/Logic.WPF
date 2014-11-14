using Logic.Core;
using Logic.Graph;
using Logic.Simulation;
using Logic.Simulation.Blocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF
{
    public static class BoolSimulationFactory
    {
        public static IDictionary<XBlock, BoolSimulation> Create(PageGraphContext context)
        {
            var simulations = new Dictionary<XBlock, BoolSimulation>();
            foreach (var block in context.OrderedBlocks)
            {
                if (block.Name == "SIGNAL")
                {
                    var simulation = new SignalSimulation();
                    simulations.Add(block, simulation);
                    // set initial state
                    simulation.State = false;
                }
                else if (block.Name == "AND")
                {
                    var simulation = new AndSimulation();
                    simulations.Add(block, simulation);
                }
                else if (block.Name == "OR")
                {
                    var simulation = new OrSimulation();
                    simulations.Add(block, simulation);
                }
            }

            // find ordered block Inputs
            foreach (var block in context.OrderedBlocks)
            {
                Debug.Print(block.Name);

                var inputs = block.Pins
                    .Where(pin => context.PinTypes[pin] == PinType.Input)
                    .SelectMany(pin =>
                    {
                        return context.Dependencies[pin]
                            .Where(dep => context.PinTypes[dep.Item1] == PinType.Output);
                    })
                    .Select(pin => pin);

                Debug.Print("\tInputs:");
                foreach (var input in inputs)
                {
                    Debug.Print("\t" + input.Item1.Owner.Name + ", inverted: " + input.Item2.ToString());
                }

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

        public static void Run(IDictionary<XBlock, BoolSimulation> simulations)
        {
            // run one cycle of OrderedBlocks simulation
            foreach (var simulation in simulations)
            {
                simulation.Value.Run();
            }
        }
    }
}
