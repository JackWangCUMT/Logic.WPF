// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Blocks
{
    public class MemorySet : XBlock
    {
        public MemorySet()
        {
            base.Database = new List<KeyValuePair<string, IProperty>>();
            base.Shapes = new List<IShape>();
            base.Pins = new List<XPin>();

            base.Name = "SR-SET";

            IProperty setProperty = new XProperty("S");
            base.Database.Add(new KeyValuePair<string, IProperty>("Set", setProperty));

            IProperty resetProperty = new XProperty("R");
            base.Database.Add(new KeyValuePair<string, IProperty>("Reset", resetProperty));

            base.Shapes.Add(
                new XText()
                {
                    X = 0.0,
                    Y = 0.0,
                    Width = 30.0,
                    Height = 20.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 14.0,
                    Text = "{0}",
                    Properties = new[] { setProperty }
                });
            base.Shapes.Add(
                new XText()
                {
                    X = 30.0,
                    Y = 0.0,
                    Width = 30.0,
                    Height = 20.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 14.0,
                    Text = "{0}",
                    Properties = new[] { resetProperty }
                });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 0.0, Width = 60.0, Height = 30.0, IsFilled = false });
            base.Shapes.Add(new XLine() { X1 = 30.0, Y1 = 0.0, X2 = 30.0, Y2 = 30.0 });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 20.0, Width = 30.0, Height = 10.0, IsFilled = true });
            base.Pins.Add(new XPin() { Name = "S", X = 15.0, Y = 0.0, PinType = PinType.Input, Owner = null });
            base.Pins.Add(new XPin() { Name = "R", X = 45.0, Y = 0.0, PinType = PinType.Input, Owner = null });
            base.Pins.Add(new XPin() { Name = "NQ", X = 15.0, Y = 30.0, PinType = PinType.Output, Owner = null });
            base.Pins.Add(new XPin() { Name = "Q", X = 45.0, Y = 30.0, PinType = PinType.Output, Owner = null });
        }
    }
}
