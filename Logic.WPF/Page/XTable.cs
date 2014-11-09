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
    public class XTable : Canvas
    {
        private IRenderer _renderer;
        private IStyle _style = null;
        private IList<IShape> _shapes;

        public XTable()
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
                new XColor() { A = 0xFF, R = 0xD3, G = 0xD3, B = 0xD3 },
                1.0);

            _shapes = new ObservableCollection<IShape>();

            double sx = 0.0;
            double sy = 811.0;

            _shapes.Add(new XLine() { X1 = sx + 30, Y1 = sy + 0.0, X2 = sx + 30, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 75, Y1 = sy + 0.0, X2 = sx + 75, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 0, Y1 = sy + 20.0, X2 = sx + 175, Y2 = sy + 20.0 });
            _shapes.Add(new XLine() { X1 = sx + 0, Y1 = sy + 40.0, X2 = sx + 175, Y2 = sy + 40.0 });
            _shapes.Add(new XLine() { X1 = sx + 0, Y1 = sy + 60.0, X2 = sx + 175, Y2 = sy + 60.0 });

            _shapes.Add(new XLine() { X1 = sx + 175, Y1 = sy + 0.0, X2 = sx + 175, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 290, Y1 = sy + 0.0, X2 = sx + 290, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 405, Y1 = sy + 0.0, X2 = sx + 405, Y2 = sy + 80.0 });

            _shapes.Add(new XLine() { X1 = sx + 405, Y1 = sy + 20.0, X2 = sx + 1260, Y2 = sy + 20.0 });
            _shapes.Add(new XLine() { X1 = sx + 405, Y1 = sy + 40.0, X2 = sx + 695, Y2 = sy + 40.0 });
            _shapes.Add(new XLine() { X1 = sx + 965, Y1 = sy + 40.0, X2 = sx + 1260, Y2 = sy + 40.0 });
            _shapes.Add(new XLine() { X1 = sx + 405, Y1 = sy + 60.0, X2 = sx + 695, Y2 = sy + 60.0 });
            _shapes.Add(new XLine() { X1 = sx + 965, Y1 = sy + 60.0, X2 = sx + 1260, Y2 = sy + 60.0 });

            _shapes.Add(new XLine() { X1 = sx + 465, Y1 = sy + 0.0, X2 = sx + 465, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 595, Y1 = sy + 0.0, X2 = sx + 595, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 640, Y1 = sy + 0.0, X2 = sx + 640, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 695, Y1 = sy + 0.0, X2 = sx + 695, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 965, Y1 = sy + 0.0, X2 = sx + 965, Y2 = sy + 80.0 });

            _shapes.Add(new XLine() { X1 = sx + 1005, Y1 = sy + 0.0, X2 = sx + 1005, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 1045, Y1 = sy + 0.0, X2 = sx + 1045, Y2 = sy + 80.0 });
            _shapes.Add(new XLine() { X1 = sx + 1100, Y1 = sy + 0.0, X2 = sx + 1100, Y2 = sy + 80.0 });
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
