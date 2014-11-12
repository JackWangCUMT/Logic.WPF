using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Memory.SR.ResetVertical
{
    public class MemoryResetVertical : XBlock
    {
        public MemoryResetVertical()
        {
            base.Shapes = new List<IShape>();
            base.Pins = new List<XPin>();

            base.Name = "SR-RESET-V";

            base.Shapes.Add(
                new XText()
                {
                    X = 0.0,
                    Y = 0.0,
                    Width = 20.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 14.0,
                    Text = "R"
                });
            base.Shapes.Add(
                new XText()
                {
                    X = 0.0,
                    Y = 30.0,
                    Width = 20.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 14.0,
                    Text = "S"
                });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 0.0, Width = 30.0, Height = 60.0, IsFilled = false });
            base.Shapes.Add(new XLine() { X1 = 0.0, Y1 = 30.0, X2 = 30.0, Y2 = 30.0 });
            base.Shapes.Add(new XRectangle() { X = 20.0, Y = 0.0, Width = 10.0, Height = 30.0, IsFilled = true });
            base.Pins.Add(new XPin() { Name = "R", X = 0.0, Y = 15.0, PinType = PinType.Input, Owner = null });
            base.Pins.Add(new XPin() { Name = "S", X = 0.0, Y = 45.0, PinType = PinType.Input, Owner = null });
            base.Pins.Add(new XPin() { Name = "Q", X = 30.0, Y = 15.0, PinType = PinType.Output, Owner = null });
            base.Pins.Add(new XPin() { Name = "NQ", X = 30.0, Y = 45.0, PinType = PinType.Output, Owner = null });
        }
    }
}
