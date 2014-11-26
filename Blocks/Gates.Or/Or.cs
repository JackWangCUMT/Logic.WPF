using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gates.Or
{
    public class Or : XBlock
    {
        public Or()
        {
            base.Database = new List<KeyValuePair<string, XProperty>>();
            base.Shapes = new List<IShape>();
            base.Pins = new List<XPin>();

            base.Name = "OR";

            var prefixProperty = new XProperty("≥");
            base.Database.Add(new KeyValuePair<string, XProperty>("Prefix", prefixProperty));

            XProperty counterProperty = new XProperty("1");
            base.Database.Add(new KeyValuePair<string, XProperty>("Counter", counterProperty));

            base.Shapes.Add(
                new XText()
                {
                    X = 0.0,
                    Y = 0.0,
                    Width = 30.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 14.0,
                    Text = "{0}{1}",
                    Properties = new[] { prefixProperty, counterProperty }
                });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 0.0, Width = 30.0, Height = 30.0, IsFilled = false });
            base.Pins.Add(new XPin() { Name = "L", X = 0.0, Y = 15.0, PinType = PinType.None, Owner = null });
            base.Pins.Add(new XPin() { Name = "R", X = 30.0, Y = 15.0, PinType = PinType.None, Owner = null });
            base.Pins.Add(new XPin() { Name = "T", X = 15.0, Y = 0.0, PinType = PinType.None, Owner = null });
            base.Pins.Add(new XPin() { Name = "B", X = 15.0, Y = 30.0, PinType = PinType.None, Owner = null });
        }
    }
}
