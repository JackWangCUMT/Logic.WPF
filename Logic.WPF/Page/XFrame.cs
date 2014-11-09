using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Logic.WPF.Page
{
    public class XFrame : Canvas
    {
        private IRenderer _renderer;
        private IStyle _style = null;
        private IList<IShape> _shapes;

        public XFrame()
        {
            _renderer = new XRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
            };

            _style = new XStyle(
                "Shape",
                new XColor() { A = 0x00, R = 0x00, G = 0x00, B = 0x00 },
                new XColor() { A = 0xFF, R = 0xA9, G = 0xA9, B = 0xA9 },
                1.0);

            _shapes = new ObservableCollection<IShape>();

            // headers
            _shapes.Add(
                new XText()
                {
                    X = 0.0,
                    Y = 0.0,
                    Width = 330.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 19.0,
                    Text = "I N P U T S"
                });
            _shapes.Add(
                new XText()
                {
                    X = 30.0 + 5.0,
                    Y = 30.0 + 0.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Designation"
                });
            _shapes.Add(
                new XText()
                {
                    X = 30.0 + 5.0,
                    Y = 30.0 + 15.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Description"
                });
            _shapes.Add(
                new XText()
                {
                    X = 30.0 + 215.0,
                    Y = 30.0 + 0.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Signal"
                });
            _shapes.Add(
                new XText()
                {
                    X = 30.0 + 215.0,
                    Y = 30.0 + 15.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Condition"
                });
            _shapes.Add(
                new XText()
                {
                    X = 330.0,
                    Y = 0.0,
                    Width = 600.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 19.0,
                    Text = "F U N C T I O N"
                });
            _shapes.Add(
                new XText()
                {
                    X = 930.0,
                    Y = 0.0,
                    Width = 330.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 19.0,
                    Text = "O U T P U T S"
                });
            _shapes.Add(
                new XText()
                {
                    X = 930.0 + 5.0,
                    Y = 30.0 + 0.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Designation"
                });
            _shapes.Add(
                new XText()
                {
                    X = 930.0 + 5.0,
                    Y = 30.0 + 15.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Description"
                });
            _shapes.Add(
                new XText()
                {
                    X = 930.0 + 215.0,
                    Y = 30.0 + 0.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Signal"
                });
            _shapes.Add(
                new XText()
                {
                    X = 930.0 + 215.0,
                    Y = 30.0 + 15.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Condition"
                });

            // numbers
            double lx = 0.0;
            double ly = 60.0;
            double rx = 1230.0;
            double ry = 60.0;
            for (int n = 1; n <= 24; n++)
            {
                _shapes.Add(
                    new XText()
                    {
                        X = lx,
                        Y = ly,
                        Width = 30.0,
                        Height = 30.0,
                        HAlignment = HAlignment.Center,
                        VAlignment = VAlignment.Center,
                        FontName = "Consolas",
                        FontSize = 15.0,
                        Text = n.ToString("00")
                    });
                _shapes.Add(
                    new XText()
                    {
                        X = rx,
                        Y = ry,
                        Width = 30.0,
                        Height = 30.0,
                        HAlignment = HAlignment.Center,
                        VAlignment = VAlignment.Center,
                        FontName = "Consolas",
                        FontSize = 15.0,
                        Text = n.ToString("00")
                    });
                ly += 30.0;
                ry += 30.0;
            }

            _shapes.Add(new XLine() { X1 = 0.0, Y1 = 0.0, X2 = 1260.0, Y2 = 0.0 });
            _shapes.Add(new XLine() { X1 = 0.0, Y1 = 30.0, X2 = 1260.0, Y2 = 30.0 });
            _shapes.Add(new XLine() { X1 = 0.0, Y1 = 780.0, X2 = 1260.0, Y2 = 780.0 });
            _shapes.Add(new XLine() { X1 = 0.0, Y1 = 811.0, X2 = 1260.0, Y2 = 811.0 });
            _shapes.Add(new XLine() { X1 = 0.0, Y1 = 891.0, X2 = 1260.0, Y2 = 891.0 });

            _shapes.Add(new XLine() { X1 = 0.0, Y1 = 0.0, X2 = 0.0, Y2 = 891.0 });
            _shapes.Add(new XLine() { X1 = 30.0, Y1 = 30.0, X2 = 30.0, Y2 = 780.0 });
            _shapes.Add(new XLine() { X1 = 240.0, Y1 = 30.0, X2 = 240.0, Y2 = 780.0 });
            _shapes.Add(new XLine() { X1 = 330.0, Y1 = 0.0, X2 = 330.0, Y2 = 780.0 });

            _shapes.Add(new XLine() { X1 = 930.0, Y1 = 0.0, X2 = 930.0, Y2 = 780.0 });
            _shapes.Add(new XLine() { X1 = 1140.0, Y1 = 30.0, X2 = 1140.0, Y2 = 780.0 });
            _shapes.Add(new XLine() { X1 = 1230.0, Y1 = 30.0, X2 = 1230.0, Y2 = 780.0 });
            _shapes.Add(new XLine() { X1 = 1260.0, Y1 = 0.0, X2 = 1260.0, Y2 = 891.0 });

            for (double y = 60.0; y < 60.0 + 25.0 * 30.0; y += 30.0)
            {
                _shapes.Add(new XLine() { X1 = 0.0, Y1 = y, X2 = 330.0, Y2 = y });
                _shapes.Add(new XLine() { X1 = 930.0, Y1 = y, X2 = 1260.0, Y2 = y });
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_renderer != null)
            {
                _renderer.DrawShapes(dc, _style, null, _shapes);
            }
        }
    }
}
