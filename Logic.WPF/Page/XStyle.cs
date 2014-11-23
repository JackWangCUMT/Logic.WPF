using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Logic.Page
{
    public class XStyle : IStyle
    {
        public string Name { get; set; }

        private IColor _fill;
        public IColor Fill
        {
            get { return _fill; }
            set
            {
                if (value != _fill)
                {
                    _fill = value;
                    if (_brush == null)
                    {
                        _brush = new SolidColorBrush(
                            Color.FromArgb(
                                (byte)_fill.A,
                                (byte)_fill.R,
                                (byte)_fill.G,
                                (byte)_fill.B));
                    }
                    else
                    {
                        _brush.Color =
                            Color.FromArgb(
                                (byte)_stroke.A,
                                (byte)_stroke.R,
                                (byte)_stroke.G,
                                (byte)_stroke.B);
                    }
                }
            }
        }

        private IColor _stroke;
        public IColor Stroke
        {
            get { return _stroke; }
            set
            {
                if (value != _stroke)
                {
                    _stroke = value;
                    if (_pen == null)
                    {
                        _pen = new Pen(
                            new SolidColorBrush(
                                Color.FromArgb(
                                    (byte)_stroke.A,
                                    (byte)_stroke.R,
                                    (byte)_stroke.G,
                                    (byte)_stroke.B)),
                                _thickness);
                    }
                    else
                    {
                        (_pen.Brush as SolidColorBrush).Color =
                            Color.FromArgb(
                                (byte)_stroke.A,
                                (byte)_stroke.R,
                                (byte)_stroke.G,
                                (byte)_stroke.B);
                        _pen.Thickness = _thickness;
                    }
                }
            }
        }

        private double _thickness;
        public double Thickness
        {
            get { return _thickness; }
            set
            {
                if (value != _thickness)
                {
                    _thickness = value;
                    if (_pen != null)
                    {
                        _pen.Thickness = _thickness;
                    }
                }
            }
        }

        private SolidColorBrush _brush;
        public object NativeFill()
        {
            return _brush;
        }

        private Pen _pen;
        public object NativeStroke()
        {
            return _pen;
        }

        public XStyle() { }

        public XStyle(string name, XColor fill, XColor stroke, double thickness)
        {
            Name = name;
            Fill = fill;
            Stroke = stroke;
            Thickness = thickness;
        }
    }
}
