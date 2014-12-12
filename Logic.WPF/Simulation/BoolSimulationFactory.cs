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

namespace Logic.Simulation
{
    public static class BoolSimulationFactory
    {
        public static IDictionary<string, Func<XBlock, BoolSimulation>> SimulationsDict
            = new Dictionary<string, Func<XBlock, BoolSimulation>>()
        {
            // Gates
            { "AND", (block) => { return new AndSimulation(null); } },
            { "INVERTER", (block) => { return new InverterSimulation(null); } },
            { "OR", (block) => { return new OrSimulation(null, GetIntPropertyValue(block, "Counter")); } },
            { "XOR", (block) => { return new XorSimulation(null); } },
            // Memory
            { "SR-RESET", (block) => { return new MemorySetResetSimulation(MemoryPriority.Reset); } },
            { "SR-RESET-V", (block) => { return new MemorySetResetSimulation(MemoryPriority.Reset); } },
            { "SR-SET", (block) => { return new MemorySetResetSimulation(MemoryPriority.Set); } },
            { "SR-SET-V", (block) => { return new MemorySetResetSimulation(MemoryPriority.Set); } },
            // Shortcut
            { "SHORTCUT", (block) => { return new ShortcutSimulation(); } },
            // Signal
            { "SIGNAL", (block) => { return new SignalSimulation(false); } },
            { "INPUT", (block) => { return new InputSimulation(false); } },
            { "OUTPUT", (block) => { return new OutputSimulation(false); } },
            // Timers
            { 
                "TIMER-OFF", 
                (block) => 
                {
                    double delay = GetDoublePropertyValue(block, "Delay");
                    string unit = GetStringPropertyValue(block, "Unit");
                    double seconds = ConvertToSeconds(delay, unit);
                    return new TimerOffSimulation(false, seconds); 
                } 
            },
            { 
                "TIMER-ON", 
                (block) => 
                { 
                    double delay = GetDoublePropertyValue(block, "Delay");
                    string unit = GetStringPropertyValue(block, "Unit");
                    double seconds = ConvertToSeconds(delay, unit);
                    return new TimerOnSimulation(false, seconds); 
                } 
            },
            { 
                "TIMER-PULSE", 
                (block) => 
                { 
                    double delay = GetDoublePropertyValue(block, "Delay");
                    string unit = GetStringPropertyValue(block, "Unit");
                    double seconds = ConvertToSeconds(delay, unit);
                    return new TimerPulseSimulation(false, seconds); 
                } 
            }
        };

        private static string GetStringPropertyValue(XBlock block, string key)
        {
            IProperty property = block.Database.Where(p => p.Key == key).FirstOrDefault().Value;
            if (property != null
                && property.Data != null
                && property.Data is string)
            {
                return property.Data as string;
            }
            else
            {
                throw new Exception(string.Format("Can not find {0} property.", key));
            }
        }

        private static int GetIntPropertyValue(XBlock block, string key)
        {
            IProperty property = block.Database.Where(p => p.Key == key).FirstOrDefault().Value;
            int value;
            if (property != null
                && property.Data != null
                && property.Data is string)
            {
                if (!int.TryParse(property.Data as string, out value))
                {
                    throw new Exception(string.Format("Invalid format of {0} property.", key));
                }
            }
            else
            {
                throw new Exception(string.Format("Can not find {0} property.", key));
            }
            return value;
        }

        private static double GetDoublePropertyValue(XBlock block, string key)
        {
            IProperty property = block.Database.Where(p => p.Key == key).FirstOrDefault().Value;
            double value;
            if (property != null
                && property.Data != null
                && property.Data is string)
            {
                if (!double.TryParse(property.Data as string, out value))
                {
                    throw new Exception(string.Format("Invalid format of {0} property.", key));
                }
            }
            else
            {
                throw new Exception(string.Format("Can not find {0} property.", key));
            }
            return value;
        }

        private static double ConvertToSeconds(double delay, string unit)
        {
            switch (unit)
            {
                // seconds
                case "s":
                    // delay is by default in seconds
                    return delay;
                // milliseconds
                case "ms":
                    // convert milliseconds to seconds
                    return delay / 1000.0;
                // minutes
                case "m":
                    // convert minutes to seconds
                    return delay * 60.0;
                // hours
                case "h":
                    // convert hours to seconds
                    return delay * 60.0 * 60.0;
                // days
                case "d":
                    // convert hours to seconds
                    return delay * 60.0 * 60.0 * 24.0;
                default:
                    throw new Exception("Invalid delay Unit property format.");
            };
        }

        public static IDictionary<XBlock, BoolSimulation> Create(PageGraphContext context)
        {
            var simulations = new Dictionary<XBlock, BoolSimulation>();
            foreach (var block in context.OrderedBlocks)
            {
                if (SimulationsDict.ContainsKey(block.Name))
                {
                    simulations.Add(block, SimulationsDict[block.Name](block));
                }
                else
                {
                    throw new Exception("Not supported block simulation.");
                }
            }

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

        public static void Run(IDictionary<XBlock, BoolSimulation> simulations, IClock clock)
        {
            foreach (var simulation in simulations)
            {
                simulation.Value.Run(clock);
            }
        }
    }
}
