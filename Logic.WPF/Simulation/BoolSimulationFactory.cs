using Logic.Core;
using Logic.Graph;
using Logic.Simulation.Blocks;
using Logic.Util;
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

            // Gates
            Register("AND", (block) => { return new AndSimulation(null); });
            Register("INVERTER", (block) => { return new InverterSimulation(null); });
            Register("OR", (block) => { return new OrSimulation(null, block.GetIntPropertyValue("Counter")); });
            Register("XOR", (block) => { return new XorSimulation(null); });
            // Memory
            Register("SR-RESET", (block) => { return new MemorySetResetSimulation(MemoryPriority.Reset); });
            Register("SR-RESET-V", (block) => { return new MemorySetResetSimulation(MemoryPriority.Reset); });
            Register("SR-SET", (block) => { return new MemorySetResetSimulation(MemoryPriority.Set); });
            Register("SR-SET-V", (block) => { return new MemorySetResetSimulation(MemoryPriority.Set); });
            // Shortcut
            Register("SHORTCUT", (block) => { return new ShortcutSimulation(); });
            // Signal
            Register("SIGNAL", (block) => { return new SignalSimulation(false); });
            Register("INPUT", (block) => { return new InputSimulation(false); });
            Register("OUTPUT", (block) => { return new OutputSimulation(false); });
            // Timers
            Register(
                "TIMER-OFF",
                (block) =>
                {
                    double delay = block.GetDoublePropertyValue("Delay");
                    string unit = block.GetStringPropertyValue("Unit");
                    double seconds = delay.ConvertToSeconds(unit);
                    return new TimerOffSimulation(false, seconds);
                });
            Register(
                "TIMER-ON",
                (block) =>
                {
                    double delay = block.GetDoublePropertyValue("Delay");
                    string unit = block.GetStringPropertyValue("Unit");
                    double seconds = delay.ConvertToSeconds(unit);
                    return new TimerOnSimulation(false, seconds);
                } );
            Register(
                "TIMER-PULSE",
                (block) =>
                {
                    double delay = block.GetDoublePropertyValue("Delay");
                    string unit = block.GetStringPropertyValue("Unit");
                    double seconds = delay.ConvertToSeconds(unit);
                    return new TimerPulseSimulation(false, seconds);
                });
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
