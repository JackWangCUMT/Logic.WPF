// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Blocks
{
    public class Or : XBlock
    {
        public Or()
        {
            base.Database = new List<KeyValuePair<string, IProperty>>();
            base.Shapes = new List<IShape>();
            base.Pins = new List<XPin>();

            base.Name = "OR";

            IProperty prefixProperty = new XProperty("≥");
            base.Database.Add(new KeyValuePair<string, IProperty>("Prefix", prefixProperty));

            IProperty counterProperty = new XProperty("1");
            base.Database.Add(new KeyValuePair<string, IProperty>("Counter", counterProperty));

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
