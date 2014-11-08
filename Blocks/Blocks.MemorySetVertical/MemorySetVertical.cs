using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blocks.MemorySetVertical
{
    [Export(typeof(XBlock))]
    public class MemorySetVertical : XBlock
    {
        public MemorySetVertical()
        {
            base.Shapes = new ObservableCollection<IShape>();
            base.Pins = new ObservableCollection<XPin>();

            base.Name = "SR-SET-V";

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
            base.Shapes.Add(new XRectangle() { X =20.0, Y = 30.0, Width = 10.0, Height = 30.0, IsFilled = true });
            base.Pins.Add(new XPin() { Name = "R", X = 0.0, Y = 15.0 });
            base.Pins.Add(new XPin() { Name = "S", X = 0.0, Y = 45.0 });
            base.Pins.Add(new XPin() { Name = "Q", X = 30.0, Y = 15.0 });
            base.Pins.Add(new XPin() { Name = "NQ", X = 30.0, Y = 45.0 });
        }
    }
}
