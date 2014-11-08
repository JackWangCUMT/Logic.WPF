using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blocks.And
{
    [Export(typeof(XBlock))]
    public class And : XBlock
    {
        public And()
        {
            base.Shapes = new ObservableCollection<IShape>();
            base.Pins = new ObservableCollection<XPin>();

            base.Name = "AND";

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
                    Text = "&"
                });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 0.0, Width = 30.0, Height = 30.0, IsFilled = false });
            base.Pins.Add(new XPin() { Name = "L", X = 0.0, Y = 15.0 });
            base.Pins.Add(new XPin() { Name = "R", X = 30.0, Y = 15.0 });
            base.Pins.Add(new XPin() { Name = "T", X = 15.0, Y = 0.0 });
            base.Pins.Add(new XPin() { Name = "B", X = 15.0, Y = 30.0 });
        }
    }
}
