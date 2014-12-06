using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Simulation.Blocks
{
    public class TimerOffSimulation : BoolSimulation
    {
        public double Delay { get; set; }

        public TimerOffSimulation()
            : base()
        {
        }

        public TimerOffSimulation(bool? state, double delay)
            : base()
        {
            base.State = state;
            this.Delay = delay;
        }

        private bool _isEnabled;
        private bool _isLowEnabled;
        private long _endCycle;

        public override void Run(IClock clock)
        {
            int length = Inputs.Length;
            if (length == 0)
            {
                // Do nothing.
            }
            else if (length == 1)
            {
                var input = Inputs[0];
                bool? enableState = input.IsInverted ? !(input.Simulation.State) : input.Simulation.State;
                switch (enableState)
                {
                    case true:
                        {
                            if (_isEnabled == false && _isLowEnabled == false)
                            {
                                base.State = true;
                                _isEnabled = true;
                                _isLowEnabled = false;
                            }
                            else if (_isEnabled == true && _isLowEnabled == true && base.State != false)
                            {
                                if (clock.Cycle >= _endCycle)
                                {
                                    base.State = false;
                                    _isEnabled = false;
                                    _isLowEnabled = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case false:
                        {
                            if (_isEnabled == true && _isLowEnabled == false)
                            {
                                // Delay -> in seconds
                                // Clock.Cycle
                                // Clock.Resolution -> in milliseconds
                                long cyclesDelay = (long)(Delay * 1000.0) / clock.Resolution;
                                _endCycle = clock.Cycle + cyclesDelay;
                                _isLowEnabled = true;
                                break;
                            }
                            else if (_isEnabled == true && _isLowEnabled == true && base.State != false)
                            {
                                if (clock.Cycle >= _endCycle)
                                {
                                    base.State = false;
                                    _isEnabled = false;
                                    _isLowEnabled = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case null:
                        {
                            _isEnabled = false;
                            _isLowEnabled = false;
                            base.State = null;
                        }
                        break;
                }
            }
            else
            {
                throw new Exception("TimerOff simulation can only have one input State.");
            }
        }
    }
}
