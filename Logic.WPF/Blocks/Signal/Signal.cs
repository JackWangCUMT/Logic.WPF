// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Blocks
{
    public class Signal : XBlock
    {
        public Signal()
        {
            base.Database = new List<KeyValuePair<string, IProperty>>();
            base.Shapes = new List<IShape>();
            base.Pins = new List<XPin>();

            base.Name = "SIGNAL";

            IProperty designationProperty = new XProperty("Designation");
            base.Database.Add(new KeyValuePair<string, IProperty>("Designation", designationProperty));

            IProperty descriptionProperty = new XProperty("Description");
            base.Database.Add(new KeyValuePair<string, IProperty>("Description", descriptionProperty));

            IProperty signalProperty = new XProperty("Signal");
            base.Database.Add(new KeyValuePair<string, IProperty>("Signal", signalProperty));

            IProperty conditionProperty = new XProperty("Condition");
            base.Database.Add(new KeyValuePair<string, IProperty>("Condition", conditionProperty));

            base.Shapes.Add(
                new XText()
                {
                    X = 5.0,
                    Y = 0.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "{0}",
                    Properties = new[] { designationProperty }
                });
            base.Shapes.Add(
                new XText()
                {
                    X = 5.0,
                    Y = 15.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "{0}",
                    Properties = new[] { descriptionProperty }
                });
            base.Shapes.Add(
                new XText()
                {
                    X = 215.0,
                    Y = 0.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "{0}",
                    Properties = new[] { signalProperty }
                });
            base.Shapes.Add(
                new XText()
                {
                    X = 215.0,
                    Y = 15.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "{0}",
                    Properties = new[] { conditionProperty }
                });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 0.0, Width = 300.0, Height = 30.0, IsFilled = false });
            base.Shapes.Add(new XLine() { X1 = 210.0, Y1 = 0.0, X2 = 210.0, Y2 = 30.0 });
            base.Pins.Add(new XPin() { Name = "I", X = 0.0, Y = 15.0, PinType = PinType.Input, Owner = null });
            base.Pins.Add(new XPin() { Name = "O", X = 300.0, Y = 15.0, PinType = PinType.Output, Owner = null });
        }
    }
}
