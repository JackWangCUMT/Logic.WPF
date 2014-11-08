using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blocks.TimerOff
{
    [Export(typeof(XBlock))]
    public class TimerOff : XBlock
    {
        public TimerOff()
        {
            base.Shapes = new ObservableCollection<IShape>();
            base.Pins = new ObservableCollection<XPin>();

            base.Name = "TIMER-OFF";

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
            base.Shapes.Add(
                new XText()
                {
                    X = 0.0,
                    Y = 3.0,
                    Width = 15.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "0"
                });
            base.Shapes.Add(
                new XText()
                {
                    X = 15.0,
                    Y = 3.0,
                    Width = 15.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "T"
                });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 0.0, Width = 30.0, Height = 30.0, IsFilled = false });
            base.Shapes.Add(new XLine() { X1 = 7.0, Y1 = 18.0, X2 = 7.0, Y2 = 22.0 });
            base.Shapes.Add(new XLine() { X1 = 23.0, Y1 = 18.0, X2 = 23.0, Y2 = 22.0 });
            base.Shapes.Add(new XLine() { X1 = 23.0, Y1 = 20.0, X2 = 7.0, Y2 = 20.0 });
            base.Pins.Add(new XPin() { Name = "L", X = 0.0, Y = 15.0 });
            base.Pins.Add(new XPin() { Name = "R", X = 30.0, Y = 15.0 });
            base.Pins.Add(new XPin() { Name = "T", X = 15.0, Y = 0.0 });
            base.Pins.Add(new XPin() { Name = "B", X = 15.0, Y = 30.0 });
        }
    }
}
