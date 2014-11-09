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
    public class XGrid : Canvas
    {
        private IRenderer _renderer;
        private IStyle _style = null;
        private IList<IShape> _shapes;

        public XGrid()
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

            double sx = 330.0;
            double sy = 30.0;
            double width = 600.0;
            double height = 750.0;
            double size = 30.0;

            for (double x = sx + size; x < sx + width; x += size)
            {
                _shapes.Add(new XLine() { X1 = x, Y1 = sy, X2 = x, Y2 = sy + height });
            }

            for (double y = sy + size; y < sy + height; y += size)
            {
                _shapes.Add(new XLine() { X1 = sx, Y1 = y, X2 = sx + width, Y2 = y });
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
