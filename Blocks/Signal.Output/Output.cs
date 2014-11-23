using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Signal.Output
{
    public class Output : XBlock
    {
        public Output()
        {
            base.Properties = new List<KeyValuePair<string, XProperty>>();
            base.Shapes = new List<IShape>();
            base.Pins = new List<XPin>();

            base.Name = "OUTPUT";

            XProperty labelProperty = new XProperty("OUT");
            base.Properties.Add(new KeyValuePair<string, XProperty>("Label", labelProperty));

            base.Shapes.Add(
                new XText()
                {
                    X = 0,
                    Y = 0,
                    Width = 30,
                    Height = 30,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 14,
                    Text = "{0}",
                    TextProperty = labelProperty
                });
            base.Shapes.Add(new XRectangle() { X = 0, Y = 0, Width = 30, Height = 30, IsFilled = false });
            base.Pins.Add(new XPin() { Name = "I", X = 0, Y = 15, PinType = PinType.Input, Owner = null });
        }
    }
}
