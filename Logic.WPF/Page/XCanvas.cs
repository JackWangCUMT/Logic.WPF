using Logic.Core;
using Logic.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Logic.WPF.Page
{
    public class XCanvas : Canvas
    {
        #region Properties

        public XLayers Layers { get; set; }
        public IList<IShape> Shapes { get; set; }
        public Tool CurrentTool { get; set; }
        public bool EnableSnap { get; set; }
        public double SnapSize { get; set; }
        public IRenderer Renderer { get; set; } 
        public XHistory<XPage> History { get; set; }

        #endregion

        #region Enums

        public enum Tool
        {
            None,
            Selection,
            Line,
            Ellipse,
            Rectangle,
            Text,
            Wire,
            Pin
        }

        public enum Mode
        {
            None,
            Selection,
            Create,
            Move
        }

        public enum Element
        {
            None,
            Line,
            Ellipse,
            Rectangle,
            Text,
            Wire,
            Pin,
            Block
        }

        public enum LineHit
        {
            None,
            Start,
            End,
            Line
        }

        #endregion

        #region Fields

        private double _startx, _starty;
        private XBlock _block = null;
        private XLine _line = null;
        private XEllipse _ellipse = null;
        private XRectangle _rectangle = null;
        private XText _text = null;
        private XWire _wire = null;
        private XPin _pin = null;
        private XRectangle _selection = null;
        private Mode _mode = Mode.None;
        private Element _element = Element.None;
        private LineHit _lineHit = LineHit.None;

        #endregion

        #region Constructor

        public XCanvas()
        {
            InitProperties();
            InitMouse();
        }

        #endregion

        #region Helpers

        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        #endregion

        #region Page

        public void Load(XPage page)
        {
            Layers.Template.Shapes = page.Template.Shapes;
            Layers.Blocks.Shapes = page.Blocks;
            Layers.Wires.Shapes = page.Wires;
            Layers.Pins.Shapes = page.Pins;

            Layers.Template.InvalidateVisual();
            Layers.Blocks.InvalidateVisual();
            Layers.Wires.InvalidateVisual();
            Layers.Pins.InvalidateVisual();
        }

        public XPage Store(string name)
        {
            return new XPage()
            {
                Name = name,
                Blocks = Layers.Blocks.Shapes,
                Pins = Layers.Pins.Shapes,
                Wires = Layers.Wires.Shapes,
                Template = new XTemplate() 
                { 
                    Shapes = Layers.Template.Shapes 
                }
            };
        }

        #endregion

        #region Initialize

        private void InitProperties()
        {
            Shapes = new ObservableCollection<IShape>();
            EnableSnap = true;
            SnapSize = 15.0;
            Renderer = new XRenderer();
        }

        private void InitMouse()
        {
            #region Left Down

            PreviewMouseLeftButtonDown += (s, e) =>
            {
                switch (_mode)
                {
                    case Mode.None:
                        if (!IsMouseCaptured)
                        {
                            switch (CurrentTool)
                            {
                                case Tool.None:
                                    break; 
                                case Tool.Selection:
                                    SelectionInit(e.GetPosition(this));
                                    break;
                                case Tool.Line:
                                case Tool.Ellipse:
                                case Tool.Rectangle:
                                case Tool.Text:
                                case Tool.Pin:
                                    CreateInit(e.GetPosition(this));
                                    break; 
                                case Tool.Wire:
                                    CreateWireInit(e.GetPosition(this));
                                    break;
                            }
                        }
                        break;
                    case Mode.Selection:
                        break;
                    case Mode.Create:
                        if (IsMouseCaptured)
                        {
                            switch (CurrentTool)
                            {
                                case Tool.None:
                                    break; 
                                case Tool.Line:
                                case Tool.Ellipse:
                                case Tool.Rectangle:
                                case Tool.Text:
                                case Tool.Pin:
                                    CreateFinish(e.GetPosition(this));
                                    break; 
                                case Tool.Wire:
                                    CreateWireFinish(e.GetPosition(this));
                                    break;
                            }
                        }
                        break;
                    case Mode.Move:
                        break; 
                }
            };

            #endregion

            #region Left Up

            PreviewMouseLeftButtonUp += (s, e) =>
            {
                if (IsMouseCaptured)
                {
                    switch (_mode)
                    {
		                case Mode.None:
                            break;
		                case Mode.Selection:
                            SelectionFinish(e.GetPosition(this));
                            break; 
                        case Mode.Create:
                            break;
                        case Mode.Move:
                            MoveFinish();
                            break;
                    }
                }
            };

            #endregion

            #region Move

            PreviewMouseMove += (s, e) =>
            {
                if (IsMouseCaptured)
                {
                    switch (_mode)
                    {
                        case Mode.None:
                            break;
                        case Mode.Selection:
                            SelectionMove(e.GetPosition(this));
                            break; 
                        case Mode.Create:
                            CreateMove(e.GetPosition(this));
                            break;
                        case Mode.Move:
                            Move(e.GetPosition(this));
                            break;
                    };
                }
            };

            #endregion

            #region Right Down

            PreviewMouseRightButtonDown += (s, e) =>
            {
                if (IsMouseCaptured)
                {
                    switch (_mode)
                    {
                        case Mode.None:
                            break;
                        case Mode.Selection:
                            SelectionCancel();
                            break; 
                        case Mode.Create:
                            CreateCancel();
                            break; 
                        case Mode.Move:
                            MoveCancel();
                            break; 
                    }
                }
            };

            #endregion
        }

        #endregion

        #region HitTest Line

        public Point NearestPointOnLine(Point a, Point b, Point p)
        {
            double ax = p.X - a.X;
            double ay = p.Y - a.Y;
            double bx = b.X - a.X;
            double by = b.Y - a.Y;
            double t = (ax * bx + ay * by) / (bx * bx + by * by);
            if (t < 0.0)
            {
                return new Point(a.X, a.Y);
            }
            else if (t > 1.0)
            {
                return new Point(b.X, b.Y);
            }
            return new Point(bx * t + a.X, by * t + a.Y);
        }

        public double Distance(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public void Middle(ref Point point, double x1, double y1, double x2, double y2)
        {
            point.X = (x1 + x2) / 2.0;
            point.Y = (y1 + y2) / 2.0;
        }

        public bool HitTest(XLine line, Point p, double treshold)
        {
            var a = new Point(line.X1, line.Y1);
            var b = new Point(line.X2, line.Y2);
            var nearest = NearestPointOnLine(a, b, p);
            double distance = Distance(p.X, p.Y, nearest.X, nearest.Y);
            return distance < treshold;
        }

        public bool HitTest(XWire wire, Point p, double treshold)
        {
            var a = wire.Start != null ? 
                new Point(wire.Start.X, wire.Start.Y) : new Point(wire.X1, wire.Y1);
            var b = wire.End != null ? 
                new Point(wire.End.X, wire.End.Y) : new Point(wire.X2, wire.Y2);
            var nearest = NearestPointOnLine(a, b, p);
            double distance = Distance(p.X, p.Y, nearest.X, nearest.Y);
            return distance < treshold;
        }

        #endregion

        #region HitTest Shapes

        public Rect GetPinBounds(double x, double y)
        {
            return new Rect(
                x - XRenderer.PinRadius,
                y - XRenderer.PinRadius,
                XRenderer.PinRadius + XRenderer.PinRadius,
                XRenderer.PinRadius + XRenderer.PinRadius);
        }

        public IShape HitTest(IEnumerable<XPin> pins, Point p)
        {
            foreach (var pin in pins)
            {
                if (GetPinBounds(pin.X, pin.Y).Contains(p))
                {
                    return pin;
                }
                continue;
            }

            return null;
        }

        public IShape HitTest(IEnumerable<XWire> wires, Point p)
        {
            foreach (var wire in wires)
            {
                var start = wire.Start;
                if (start != null)
                {
                    if (GetPinBounds(start.X, start.Y).Contains(p))
                    {
                        return start;
                    }
                }
                else
                {
                    if (GetPinBounds(wire.X1, wire.Y1).Contains(p))
                    {
                        return wire;
                    }
                }

                var end = wire.End;
                if (end != null)
                {
                    if (GetPinBounds(end.X, end.Y).Contains(p))
                    {
                        return end;
                    }
                }
                else
                {
                    if (GetPinBounds(wire.X2, wire.Y2).Contains(p))
                    {
                        return wire;
                    }
                }

                if (HitTest(wire, p, XRenderer.HitTreshold))
                {
                    return wire;
                }
            }

            return null;
        }

        public IShape HitTest(IEnumerable<XBlock> blocks, Point p)
        {
            foreach (var block in blocks)
            {
                var pin = HitTest(block.Pins, p);
                if (pin != null)
                {
                    return pin;
                }

                var shape = HitTest(block.Shapes, p);
                if (shape != null)
                {
                    return block;
                }
            }

            return null;
        }

        public IShape HitTest(IEnumerable<IShape> shapes, Point p)
        {
            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    var line = shape as XLine;

                    if (GetPinBounds(line.X1, line.Y1).Contains(p))
                    {
                        _lineHit = LineHit.Start;
                        return line;
                    }

                    if (GetPinBounds(line.X2, line.Y2).Contains(p))
                    {
                        _lineHit = LineHit.End;
                        return line;
                    }

                    if (HitTest(line, p, XRenderer.HitTreshold))
                    {
                        _lineHit = LineHit.Line;
                        return line;
                    }

                    continue;
                }
                else if (shape is XEllipse)
                {
                    var ellipse = shape as XEllipse;
                    var bounds = new Rect(
                        ellipse.X - ellipse.RadiusX,
                        ellipse.Y - ellipse.RadiusY,
                        ellipse.RadiusX + ellipse.RadiusX,
                        ellipse.RadiusY + ellipse.RadiusY);
                    if (bounds.Contains(p))
                    {
                        return shape;
                    }
                    continue;
                }
                else if (shape is XRectangle)
                {
                    var rectangle = shape as XRectangle;
                    var bounds = new Rect(
                        rectangle.X, 
                        rectangle.Y, 
                        rectangle.Width, 
                        rectangle.Height);
                    if (bounds.Contains(p))
                    {
                        return shape;
                    }
                    continue;
                }
                else if (shape is XText)
                {
                    var text = shape as XText;
                    var bounds = new Rect(
                        text.X, 
                        text.Y, 
                        text.Width, 
                        text.Height);
                    if (bounds.Contains(p))
                    {
                        return shape;
                    }
                    continue;
                }
            }

            return null;
        }

        public IShape HitTest(Point p)
        {
            var pin = HitTest(Layers.Pins.Shapes.Cast<XPin>(), p);
            if (pin != null)
            {
                return pin;
            }

            var wire = HitTest(Layers.Wires.Shapes.Cast<XWire>(), p);
            if (wire != null)
            {
                return wire;
            }

            var block = HitTest(Layers.Blocks.Shapes.Cast<XBlock>(), p);
            if (block != null)
            {
                if (block is XPin)
                {
                    return null;
                }
                else
                {
                    return block;
                }
            }

            var template = HitTest(Layers.Template.Shapes, p);
            if (template != null)
            {
                return template;
            }

            return null;
        } 

        #endregion

        #region Selection Mode

        private void SelectionInit(Point p)
        {
            IShape shape = Layers != null ? HitTest(p) : null;
            if (shape != null)
            {
                MoveInit(shape, p);
            }
            else
            {
                _startx = p.X;
                _starty = p.Y;
                _selection = new XRectangle()
                {
                    X = p.X,
                    Y = p.Y,
                    Width = 0.0,
                    Height = 0.0,
                    IsFilled = true
                };
                Shapes.Add(_selection);
                CaptureMouse();
                InvalidateVisual();
                _mode = Mode.Selection;
            }
        }

        private void SelectionMove(Point p)
        {
            _selection.X = Math.Min(_startx, p.X);
            _selection.Y = Math.Min(_starty, p.Y);
            _selection.Width = Math.Abs(p.X - _startx);
            _selection.Height = Math.Abs(p.Y - _starty);
            InvalidateVisual();
        }

        private void SelectionFinish(Point p)
        {
            var rect = new Rect(
                Math.Min(_startx, p.X),
                Math.Min(_starty, p.Y),
                Math.Abs(p.X - _startx),
                Math.Abs(p.Y - _starty));

            // TODO:

            ReleaseMouseCapture();
            Shapes.Remove(_selection);
            InvalidateVisual();
            _mode = Mode.None;
        }

        private void SelectionCancel()
        {
            ReleaseMouseCapture();
            Shapes.Remove(_selection);
            InvalidateVisual();
            _mode = Mode.None;
        }

        #endregion

        #region Move Mode

        private void MoveInit(IShape shape, Point p)
        {
            if (EnableSnap)
            {
                _startx = Snap(p.X, SnapSize);
                _starty = Snap(p.Y, SnapSize);
            }
            else
            {
                _startx = p.X;
                _starty = p.Y;
            }

            if (shape is XLine)
            {
                _element = Element.Line;
                _line = shape as XLine;
            }
            else if (shape is XEllipse)
            {
                _element = Element.Ellipse;
                _ellipse = shape as XEllipse;
            }
            else if (shape is XRectangle)
            {
                _element = Element.Rectangle;
                _rectangle = shape as XRectangle;
            }
            else if (shape is XText)
            {
                _element = Element.Text;
                _text = shape as XText;
            }
            else if (shape is XWire)
            {
                _element = Element.Wire;
                _wire = shape as XWire;
            }
            else if (shape is XPin)
            {
                _element = Element.Pin;
                _pin = shape as XPin;
            }
            else if (shape is XBlock)
            {
                _element = Element.Block;
                _block = shape as XBlock;
            }

            CaptureMouse();
            _mode = Mode.Move;
        }

        private void MoveFinish()
        {
            ReleaseMouseCapture();
            _mode = Mode.None;
        }

        private void MoveCancel()
        {
            ReleaseMouseCapture();
            _mode = Mode.None;
        }

        private void Move(Point p)
        {
            double dx, dy;

            if (EnableSnap)
            {
                double x = Snap(p.X, SnapSize);
                double y = Snap(p.Y, SnapSize);
                dx = x - _startx;
                dy = y - _starty;
                _startx = x;
                _starty = y;
            }
            else
            {
                dx = p.X - _startx;
                dy = p.Y - _starty;
                _startx = p.X;
                _starty = p.Y;
            }

            switch (_element)
            {
                case Element.Line:
                    switch(_lineHit)
                    {
                        case LineHit.Start:
                            _line.X1 += dx;
                            _line.Y1 += dy;
                            break;
                        case LineHit.End:
                            _line.X2 += dx;
                            _line.Y2 += dy;
                            break;
                        case LineHit.Line:
                            _line.X1 += dx;
                            _line.Y1 += dy;
                            _line.X2 += dx;
                            _line.Y2 += dy;
                            break;
                    }
                    Layers.Template.InvalidateVisual();
                    break;
                case Element.Ellipse:
                    _ellipse.X += dx;
                    _ellipse.Y += dy;
                    Layers.Template.InvalidateVisual();
                    break;
                case Element.Rectangle:
                    _rectangle.X += dx;
                    _rectangle.Y += dy;
                    Layers.Template.InvalidateVisual();
                    break;
                case Element.Text:
                    _text.X += dx;
                    _text.Y += dy;
                    Layers.Template.InvalidateVisual();
                    break;
                case Element.Wire:
                    // TODO:
                    break;
                case Element.Pin:
                    _pin.X += dx;
                    _pin.Y += dy;
                    Layers.Pins.InvalidateVisual();
                    Layers.Wires.InvalidateVisual();
                    break;
                case Element.Block:
                    XCanvas.Move(_block, dx, dy);
                    Layers.Blocks.InvalidateVisual();
                    Layers.Pins.InvalidateVisual();
                    Layers.Wires.InvalidateVisual();
                    break;
            }
        }

        public static void Move(XBlock block, double dx, double dy)
        {
            foreach (var shape in block.Shapes)
            {
                if (shape is XLine)
                {
                    var line = shape as XLine;
                    line.X1 += dx;
                    line.Y1 += dy;
                    line.X2 += dx;
                    line.Y2 += dy;
                }
                else if (shape is XEllipse)
                {
                    var ellipse = shape as XEllipse;
                    ellipse.X += dx;
                    ellipse.Y += dy;
                }
                else if (shape is XRectangle)
                {
                    var rectangle = shape as XRectangle;
                    rectangle.X += dx;
                    rectangle.Y += dy;
                }
                else if (shape is XText)
                {
                    var text = shape as XText;
                    text.X += dx;
                    text.Y += dy;
                }
                else if (shape is XWire)
                {
                    var wire = shape as XWire;
                    wire.X1 += dx;
                    wire.Y1 += dy;
                    wire.X2 += dx;
                    wire.Y2 += dy;
                }
                else if (shape is XPin)
                {
                    var pin = shape as XPin;
                    pin.X += dx;
                    pin.Y += dy;
                }
            }

            foreach (var pin in block.Pins)
            {
                pin.X += dx;
                pin.Y += dy;
            }
        }

        #endregion

        #region Create Mode

        private void CreateWireInit(Point p)
        {
            IShape pin = null;
            IShape wire = null;
            IShape block = null;

            if (Layers != null)
            {
                pin = HitTest(Layers.Pins.Shapes.Cast<XPin>(), p);
                if (pin == null)
                {
                    wire = HitTest(Layers.Wires.Shapes.Cast<XWire>(), p);
                    if (wire == null)
                    {
                        block = HitTest(Layers.Blocks.Shapes.Cast<XBlock>(), p);
                    }
                }
            }

            CreateInit(p);

            double x, y;
            if (EnableSnap)
            {
                x = Snap(p.X, SnapSize);
                y = Snap(p.Y, SnapSize);
            }
            else
            {
                x = p.X;
                y = p.Y;
            }

            if (pin == null
                && wire == null
                && (block == null || (block != null && !(block is XPin))))
            {
                if (Layers.Pins != null)
                {
                    pin = new XPin()
                    {
                        X = x,
                        Y = y,
                    };

                    Layers.Pins.Shapes.Add(pin);
                    Layers.Pins.InvalidateVisual();
                }
            }

            if (pin != null
                || wire != null
                || (block != null && block is XPin))
            {
                // connect wire start
                if (pin != null)
                {
                    _wire.Start = pin as XPin;
                }
                else if (wire != null)
                {
                    // split wire
                    if (Layers.Pins != null && Layers.Wires != null)
                    {
                        pin = new XPin()
                        {
                            X = x,
                            Y = y,
                        };

                        var split = new XWire()
                        {
                            Start = pin as XPin,
                            End = (wire as XWire).End,
                            InvertStart = false,
                            InvertEnd = (wire as XWire).InvertEnd
                        };

                        (wire as XWire).InvertEnd = false;
                        (wire as XWire).End = pin as XPin;

                        _wire.Start = pin as XPin;

                        Layers.Pins.Shapes.Add(pin);
                        Layers.Wires.Shapes.Add(split);

                        Layers.Pins.InvalidateVisual();
                        Layers.Wires.InvalidateVisual();
                    }
                }
                else if (block != null && block is XPin)
                {
                    _wire.Start = block as XPin;
                }
            }
        }

        private void CreateWireFinish(Point p)
        {
            IShape pin = null;
            IShape wire = null;
            IShape block = null;

            if (Layers != null)
            {
                pin = HitTest(Layers.Pins.Shapes.Cast<XPin>(), p);
                if (pin == null)
                {
                    wire = HitTest(Layers.Wires.Shapes.Cast<XWire>(), p);
                    if (wire == null)
                    {
                        block = HitTest(Layers.Blocks.Shapes.Cast<XBlock>(), p);
                    }
                }
            }

            CreateFinish(p);

            double x, y;
            if (EnableSnap)
            {
                x = Snap(p.X, SnapSize);
                y = Snap(p.Y, SnapSize);
            }
            else
            {
                x = p.X;
                y = p.Y;
            }

            if (pin == null
                && wire == null
                && (block == null || (block != null && !(block is XPin))))
            {
                if (Layers.Pins != null)
                {
                    pin = new XPin()
                    {
                        X = x,
                        Y = y,
                    };

                    Layers.Pins.Shapes.Add(pin);
                    Layers.Pins.InvalidateVisual();
                }
            }

            // connect wire end
            if (pin != null)
            {
                _wire.End = pin as XPin;
            }
            else if (wire != null)
            {
                // split wire
                if (Layers.Pins != null && Layers.Wires != null)
                {
                    pin = new XPin()
                    {
                        X = x,
                        Y = y,
                    };

                    var split = new XWire()
                    {
                        Start = pin as XPin,
                        End = (wire as XWire).End,
                        InvertStart = false,
                        InvertEnd = (wire as XWire).InvertEnd
                    };

                    (wire as XWire).InvertEnd = false;
                    (wire as XWire).End = pin as XPin;

                    _wire.End = pin as XPin;

                    Layers.Pins.Shapes.Add(pin);
                    Layers.Wires.Shapes.Add(split);

                    Layers.Pins.InvalidateVisual();
                    Layers.Wires.InvalidateVisual();
                }
            }
            else if (block != null && block is XPin)
            {
                _wire.End = block as XPin;
            }
        }

        private void CreateInit(Point p)
        {
            double x, y;
            if (EnableSnap)
            {
                x = Snap(p.X, SnapSize);
                y = Snap(p.Y, SnapSize);
            }
            else
            {
                x = p.X;
                y = p.Y;
            }

            switch (CurrentTool)
            {
                case Tool.Line:
                    {
                        _line = new XLine()
                        {
                            X1 = x,
                            Y1 = y,
                            X2 = x,
                            Y2 = y
                        };
                        Shapes.Add(_line);
                        CaptureMouse();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Ellipse:
                    {
                        _startx = x;
                        _starty = y;
                        _ellipse = new XEllipse()
                        {
                            X = x,
                            Y = y,
                            RadiusX = 0.0,
                            RadiusY = 0.0
                        };
                        Shapes.Add(_ellipse);
                        CaptureMouse();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Rectangle:
                    {
                        _startx = x;
                        _starty = y;
                        _rectangle = new XRectangle()
                        {
                            X = x,
                            Y = y,
                            Width = 0.0,
                            Height = 0.0
                        };
                        Shapes.Add(_rectangle);
                        CaptureMouse();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Text:
                    {
                        _startx = x;
                        _starty = y;
                        _text = new XText()
                        {
                            Text = "Text",
                            X = x,
                            Y = y,
                            Width = 0.0,
                            Height = 0.0,
                            IsFilled = false,
                            HAlignment = HAlignment.Center,
                            VAlignment = VAlignment.Center,
                            FontName = "Consolas",
                            FontSize = 12.0
                        };
                        Shapes.Add(_text);
                        CaptureMouse();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Wire:
                    {
                        _wire = new XWire()
                        {
                            X1 = x,
                            Y1 = y,
                            X2 = x,
                            Y2 = y,
                            InvertStart = false,
                            InvertEnd = false
                        };
                        Shapes.Add(_wire);
                        CaptureMouse();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Pin:
                    {
                        _pin = new XPin()
                        {
                            X = x,
                            Y = y,
                        };
                        Shapes.Add(_pin);
                        CaptureMouse();
                        InvalidateVisual();
                    }
                    break;
            }
            _mode = Mode.Create;
        }

        private void CreateMove(Point p)
        {
            double x, y;
            if (EnableSnap)
            {
                x = Snap(p.X, SnapSize);
                y = Snap(p.Y, SnapSize);
            }
            else
            {
                x = p.X;
                y = p.Y;
            }

            switch (CurrentTool)
            {
                case Tool.Line:
                    {
                        _line.X2 = x;
                        _line.Y2 = y;
                        InvalidateVisual();
                    }
                    break;
                case Tool.Ellipse:
                    {
                        _ellipse.RadiusX = Math.Abs(x - _startx) / 2.0;
                        _ellipse.RadiusY = Math.Abs(y - _starty) / 2.0;
                        _ellipse.X = Math.Min(_startx, x) + _ellipse.RadiusX;
                        _ellipse.Y = Math.Min(_starty, y) + _ellipse.RadiusY;
                        InvalidateVisual();
                    }
                    break;
                case Tool.Rectangle:
                    {
                        _rectangle.X = Math.Min(_startx, x);
                        _rectangle.Y = Math.Min(_starty, y);
                        _rectangle.Width = Math.Abs(x - _startx);
                        _rectangle.Height = Math.Abs(y - _starty);
                        InvalidateVisual();
                    }
                    break;
                case Tool.Text:
                    {
                        _text.X = Math.Min(_startx, x);
                        _text.Y = Math.Min(_starty, y);
                        _text.Width = Math.Abs(x - _startx);
                        _text.Height = Math.Abs(y - _starty);
                        InvalidateVisual();
                    }
                    break;
                case Tool.Wire:
                    {
                        _wire.X2 = x;
                        _wire.Y2 = y;
                        InvalidateVisual();
                    }
                    break;
                case Tool.Pin:
                    {
                        _pin.X = x;
                        _pin.Y = y;
                        InvalidateVisual();
                    }
                    break;
            }
        }

        private void CreateFinish(Point p)
        {
            double x, y;
            if (EnableSnap)
            {
                x = Snap(p.X, SnapSize);
                y = Snap(p.Y, SnapSize);
            }
            else
            {
                x = p.X;
                y = p.Y;
            }

            switch (CurrentTool)
            {
                case Tool.Line:
                    {
                        _line.X2 = x;
                        _line.Y2 = y;
                        if (Layers.Template != null)
                        {
                            Shapes.Remove(_line);
                            if (History != null)
                            {
                                History.Snapshot(Store("Page"));
                            }
                            Layers.Template.Shapes.Add(_line);
                            Layers.Template.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Ellipse:
                    {
                        _ellipse.RadiusX = Math.Abs(x - _startx) / 2.0;
                        _ellipse.RadiusY = Math.Abs(y - _starty) / 2.0;
                        _ellipse.X = Math.Min(_startx, x) + _ellipse.RadiusX;
                        _ellipse.Y = Math.Min(_starty, y) + _ellipse.RadiusY;
                        if (Layers.Template != null)
                        {
                            Shapes.Remove(_ellipse);
                            if (History != null)
                            {
                                History.Snapshot(Store("Page"));
                            }
                            Layers.Template.Shapes.Add(_ellipse);
                            Layers.Template.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Rectangle:
                    {
                        _rectangle.X = Math.Min(_startx, x);
                        _rectangle.Y = Math.Min(_starty, y);
                        _rectangle.Width = Math.Abs(x - _startx);
                        _rectangle.Height = Math.Abs(y - _starty);
                        if (Layers.Template != null)
                        {
                            Shapes.Remove(_rectangle);
                            if (History != null)
                            {
                                History.Snapshot(Store("Page"));
                            }
                            Layers.Template.Shapes.Add(_rectangle);
                            Layers.Template.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Text:
                    {
                        _text.X = Math.Min(_startx, x);
                        _text.Y = Math.Min(_starty, y);
                        _text.Width = Math.Abs(x - _startx);
                        _text.Height = Math.Abs(y - _starty);
                        if (Layers.Template != null)
                        {
                            Shapes.Remove(_text);
                            if (History != null)
                            {
                                History.Snapshot(Store("Page"));
                            }
                            Layers.Template.Shapes.Add(_text);
                            Layers.Template.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Wire:
                    {
                        _wire.X2 = x;
                        _wire.Y2 = y;
                        if (Layers.Wires != null)
                        {
                            Shapes.Remove(_wire);
                            if (History != null)
                            {
                                History.Snapshot(Store("Page"));
                            }
                            Layers.Wires.Shapes.Add(_wire);
                            Layers.Wires.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case Tool.Pin:
                    {
                        _pin.X = x;
                        _pin.Y = y;
                        if (Layers.Pins != null)
                        {
                            Shapes.Remove(_pin);
                            if (History != null)
                            {
                                History.Snapshot(Store("Page"));
                            }
                            Layers.Pins.Shapes.Add(_pin);
                            Layers.Pins.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
            }

            _mode = Mode.None;
        }

        private void CreateCancel()
        {
            switch (CurrentTool)
            {
                case Tool.Line:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_line);
                        InvalidateVisual();
                    }
                    break;
                case Tool.Ellipse:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_ellipse);
                        InvalidateVisual();
                    }
                    break;
                case Tool.Rectangle:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_rectangle);
                        InvalidateVisual();
                    }
                    break;
                case Tool.Text:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_text);
                        InvalidateVisual();
                    }
                    break;
                case Tool.Wire:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_wire);
                        InvalidateVisual();
                    }
                    break;
                case Tool.Pin:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_pin);
                        InvalidateVisual();
                    }
                    break;
            }

            _mode = Mode.None;
        }

        #endregion

        #region Create Mode Properties

        public void ToggleFill()
        {
            if (IsMouseCaptured && CurrentTool == Tool.Rectangle)
            {
                _rectangle.IsFilled = !_rectangle.IsFilled;
                InvalidateVisual();
            }

            if (IsMouseCaptured && CurrentTool == Tool.Ellipse)
            {
                _ellipse.IsFilled = !_ellipse.IsFilled;
                InvalidateVisual();
            }

            if (IsMouseCaptured && CurrentTool == Tool.Text)
            {
                _text.IsFilled = !_text.IsFilled;
                InvalidateVisual();
            }
        }

        public void ToggleInvertStart()
        {
            if (IsMouseCaptured && CurrentTool == Tool.Wire)
            {
                _wire.InvertStart = !_wire.InvertStart;
                InvalidateVisual();
            }
        }

        public void ToggleInvertEnd()
        {
            if (IsMouseCaptured && CurrentTool == Tool.Wire)
            {
                _wire.InvertEnd = !_wire.InvertEnd;
                InvalidateVisual();
            }
        }

        public void SetTextSizeDelta(double delta)
        {
            if (IsMouseCaptured && CurrentTool == Tool.Text)
            {
                double size = _text.FontSize + delta;
                if (size > 0.0)
                {
                    _text.FontSize = size;
                    InvalidateVisual();
                }
            }
        }

        public void SetTextHAlignment(HAlignment halignment)
        {
            if (IsMouseCaptured && CurrentTool == Tool.Text)
            {
                _text.HAlignment = halignment;
                InvalidateVisual();
            }
        }

        public void SetTextVAlignment(VAlignment valignment)
        {
            if (IsMouseCaptured && CurrentTool == Tool.Text)
            {
                _text.VAlignment = valignment;
                InvalidateVisual();
            }
        }

        #endregion

        #region Render

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_mode == Mode.Selection)
            {
                DrawSelection(dc, _selection);
            }
            else
            {
                DrawShapes(dc, Shapes);
            }
        }

        private void DrawShapes(object dc, IList<IShape> shapes)
        {
            var gs = new GuidelineSet(
                new double[] { 1.0, 1.0 },
                new double[] { 1.0, 1.0 });
            (dc as DrawingContext).PushGuidelineSet(gs);

            foreach (var shape in shapes)
            {
                shape.Render(dc, Renderer);
            }

            (dc as DrawingContext).Pop();
        }

        private void DrawSelection(object dc, XRectangle rectangle)
        {
            double thickness = 1.0;
            double half = thickness / 2.0;

            var gs = new GuidelineSet(
                new double[] 
                    { 
                        rectangle.X + half, 
                        rectangle.X + rectangle.Width + half 
                    },
                new double[] 
                    { 
                        rectangle.Y + half,
                        rectangle.Y + rectangle.Height + half
                    });
            (dc as DrawingContext).PushGuidelineSet(gs);

            (dc as DrawingContext).DrawRectangle(
                rectangle.IsFilled ?
                new SolidColorBrush(Color.FromArgb(0x1F, 0x00, 0x00, 0xFF)) : null,
                new Pen(
                    new SolidColorBrush(Color.FromArgb(0x9F, 0x00, 0x00, 0xFF)),
                    thickness),
                new Rect(
                    rectangle.X,
                    rectangle.Y,
                    rectangle.Width,
                    rectangle.Height));

            (dc as DrawingContext).Pop();
        } 

        #endregion
    }
}
