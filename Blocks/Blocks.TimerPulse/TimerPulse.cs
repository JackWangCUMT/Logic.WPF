using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blocks.TimerPulse
{
    [Export(typeof(XBlock))]
    public class TimerPulse : XBlock
    {
        public TimerPulse()
        {
            base.Shapes = new ObservableCollection<IShape>();
            base.Pins = new ObservableCollection<XPin>();

            base.Name = "TIMER-PULSE";

            base.Shapes.Add(
                new XText()
                {
                    X = -15.0,
                    Y = -15.0,
                    Width = 60.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "T=1s"
                });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 0.0, Width = 30.0, Height = 30.0, IsFilled = false });
            base.Shapes.Add(new XLine() { X1 = 7.0, Y1 = 19.0, X2 = 11.0, Y2 = 19.0 });
            base.Shapes.Add(new XLine() { X1 = 19.0, Y1 = 19.0, X2 = 23.0, Y2 = 19.0 });
            base.Shapes.Add(new XLine() { X1 = 11.0, Y1 = 11.0, X2 = 19.0, Y2 = 11.0 });
            base.Shapes.Add(new XLine() { X1 = 11.0, Y1 = 11.0, X2 = 11.0, Y2 = 19.0 });
            base.Shapes.Add(new XLine() { X1 = 19.0, Y1 = 11.0, X2 = 19.0, Y2 = 19.0 });
            base.Pins.Add(new XPin() { Name = "L", X = 0.0, Y = 15.0, PinType = PinType.None, Owner = null });
            base.Pins.Add(new XPin() { Name = "R", X = 30.0, Y = 15.0, PinType = PinType.None, Owner = null });
            base.Pins.Add(new XPin() { Name = "T", X = 15.0, Y = 0.0, PinType = PinType.None, Owner = null });
            base.Pins.Add(new XPin() { Name = "B", X = 15.0, Y = 30.0, PinType = PinType.None, Owner = null });
        }
    }
}
