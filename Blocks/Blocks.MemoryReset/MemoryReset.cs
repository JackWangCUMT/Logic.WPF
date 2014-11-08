using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blocks.MemoryReset
{
    [Export(typeof(XBlock))]
    public class MemoryReset : XBlock
    {
        public MemoryReset()
        {
            base.Shapes = new ObservableCollection<IShape>();
            base.Pins = new ObservableCollection<XPin>();

            base.Name = "SR-RESET";

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
                    Text = "S"
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
                    Text = "R"
                });
            base.Shapes.Add(new XRectangle() { X = 0.0, Y = 0.0, Width = 60.0, Height = 30.0, IsFilled = false });
            base.Shapes.Add(new XLine() { X1 = 30.0, Y1 = 0.0, X2 = 30.0, Y2 = 30.0 });
            base.Shapes.Add(new XRectangle() { X = 30.0, Y = 20.0, Width = 30.0, Height = 10.0, IsFilled = true });
            base.Pins.Add(new XPin() { Name = "S", X = 15.0, Y = 0.0 });
            base.Pins.Add(new XPin() { Name = "R", X = 45.0, Y = 0.0 });
            base.Pins.Add(new XPin() { Name = "NQ", X = 15.0, Y = 30.0 });
            base.Pins.Add(new XPin() { Name = "Q", X = 45.0, Y = 30.0 });
        }
    }
}
