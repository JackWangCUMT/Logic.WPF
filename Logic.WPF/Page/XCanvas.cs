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
            Selected,
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

        private IStyle _shapeStyle { get; set; }
        private IStyle _selectedShapeStyle { get; set; }
        private IStyle _selectionStyle { get; set; }

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
            InitStyles();
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

            InvalidatePage();
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

        public void InitStyles()
        {
            _shapeStyle = new XStyle(
                "Shape",
                new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                2.0);

            _selectedShapeStyle = new XStyle(
                "Selected",
                new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                2.0);

            _selectionStyle = new XStyle(
                "Selection",
                new XColor() { A = 0x1F, R = 0x00, G = 0x00, B = 0xFF },
                new XColor() { A = 0x9F, R = 0x00, G = 0x00, B = 0xFF },
                1.0);
        }

        private void InitProperties()
        {
            Shapes = new ObservableCollection<IShape>();
            EnableSnap = true;
            SnapSize = 15.0;
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
                                    SelectionReset();
                                    break; 
                                case Tool.Selection:
                                    SelectionInit(e.GetPosition(this));
                                    break;
                                case Tool.Line:
                                case Tool.Ellipse:
                                case Tool.Rectangle:
                                case Tool.Text:
                                case Tool.Pin:
                                    SelectionReset();
                                    CreateInit(e.GetPosition(this));
                                    break; 
                                case Tool.Wire:
                                    SelectionReset();
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
                Cancel();
            };

            #endregion
        }

        #endregion

        #region Cancel

        public void Cancel()
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
            else
            {
                SelectionReset();
            }
        }

        #endregion

        #region Delete

        public void Delete(IEnumerable<IShape> shapes)
        {
            foreach (var shape in Renderer.Selected)
            {
                if (shape is XLine)
                {
                    Layers.Template.Shapes.Remove(shape);
                }
                else if (shape is XEllipse)
                {
                    Layers.Template.Shapes.Remove(shape);
                }
                else if (shape is XRectangle)
                {
                    Layers.Template.Shapes.Remove(shape);
                }
                else if (shape is XText)
                {
                    Layers.Template.Shapes.Remove(shape);
                }
                else if (shape is XWire)
                {
                    Layers.Wires.Shapes.Remove(shape);
                }
                else if (shape is XPin)
                {
                    Layers.Pins.Shapes.Remove(shape);
                }
                else if (shape is XBlock)
                {
                    Layers.Blocks.Shapes.Remove(shape);
                }
            }
        }

        #endregion

        #region Point Math

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

        #endregion

        #region HitTest Bounds

        public Rect GetPinBounds(double x, double y)
        {
            return new Rect(
                x - XRenderer.PinRadius,
                y - XRenderer.PinRadius,
                XRenderer.PinRadius + XRenderer.PinRadius,
                XRenderer.PinRadius + XRenderer.PinRadius);
        }

        private static Rect GetEllipseBounds(XEllipse ellipse)
        {
            var bounds = new Rect(
                ellipse.X - ellipse.RadiusX,
                ellipse.Y - ellipse.RadiusY,
                ellipse.RadiusX + ellipse.RadiusX,
                ellipse.RadiusY + ellipse.RadiusY);
            return bounds;
        }

        private static Rect GetRectangleBounds(XRectangle rectangle)
        {
            var bounds = new Rect(
                rectangle.X,
                rectangle.Y,
                rectangle.Width,
                rectangle.Height);
            return bounds;
        }

        private static Rect GetTextBounds(XText text)
        {
            var bounds = new Rect(
                text.X,
                text.Y,
                text.Width,
                text.Height);
            return bounds;
        }

        #endregion

        #region HitTest Point

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
                    if (GetEllipseBounds(shape as XEllipse).Contains(p))
                    {
                        return shape;
                    }
                    continue;
                }
                else if (shape is XRectangle)
                {
                    if (GetRectangleBounds(shape as XRectangle).Contains(p))
                    {
                        return shape;
                    }
                    continue;
                }
                else if (shape is XText)
                {
                    if (GetTextBounds(shape as XText).Contains(p))
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

        #region HitTest Rect

        public bool HitTest(IEnumerable<XPin> pins, Rect rect, ICollection<IShape> hs)
        {
            foreach (var pin in pins)
            {
                if (GetPinBounds(pin.X, pin.Y).IntersectsWith(rect))
                {
                    if (hs != null)
                    {
                        hs.Add(pin);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<XWire> wires, Rect rect, ICollection<IShape> hs)
        {
            foreach (var wire in wires)
            {
                var start = wire.Start;
                if (start != null)
                {
                    if (GetPinBounds(start.X, start.Y).IntersectsWith(rect))
                    {
                        if (hs != null)
                        {
                            hs.Add(wire);
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (GetPinBounds(wire.X1, wire.Y1).IntersectsWith(rect))
                    {
                        if (hs != null)
                        {
                            hs.Add(wire);
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }

                var end = wire.End;
                if (end != null)
                {
                    if (GetPinBounds(end.X, end.Y).IntersectsWith(rect))
                    {
                        if (hs != null)
                        {
                            hs.Add(wire);
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (GetPinBounds(wire.X2, wire.Y2).IntersectsWith(rect))
                    {
                        if (hs != null)
                        {
                            hs.Add(wire);
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }

                // TODO:
                //if (HitTest(wire, p, XRenderer.HitTreshold))
                //{
                //    if (hs != null)
                //    {
                //        hs.Add(wire);
                //        continue;
                //    }
                //    else
                //    {
                //        return true;
                //    }
                //}
            }

            return false;
        }

        public bool HitTest(IEnumerable<XBlock> blocks, Rect rect, ICollection<IShape> hs)
        {
            foreach (var block in blocks)
            {
                bool pinHitResults = HitTest(block.Pins, rect, null);
                if (pinHitResults == true)
                {
                    if (hs != null)
                    {
                        hs.Add(block);
                    }
                    else
                    {
                        return true;
                    }
                }

                bool shapeHitResult = HitTest(block.Shapes, rect, null);
                if (shapeHitResult == true)
                {
                    if (hs != null)
                    {
                        hs.Add(block);
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HitTest(IEnumerable<IShape> shapes, Rect rect, ICollection<IShape> hs)
        {
            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    var line = shape as XLine;
                    if (GetPinBounds(line.X1, line.Y1).IntersectsWith(rect)
                        || GetPinBounds(line.X2, line.Y2).IntersectsWith(rect)
                        // TODO:
                        /* || HitTest(line, p, XRenderer.HitTreshold) */)
                    {
                        if (hs != null)
                        {
                            hs.Add(line);
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    continue;
                }
                else if (shape is XEllipse)
                {
                    if (GetEllipseBounds(shape as XEllipse).IntersectsWith(rect))
                    {
                        if (hs != null)
                        {
                            hs.Add(shape);
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    continue;
                }
                else if (shape is XRectangle)
                {
                    if (GetRectangleBounds(shape as XRectangle).IntersectsWith(rect))
                    {
                        if (hs != null)
                        {
                            hs.Add(shape);
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    continue;
                }
                else if (shape is XText)
                {
                    if (GetTextBounds(shape as XText).IntersectsWith(rect))
                    {
                        if (hs != null)
                        {
                            hs.Add(shape);
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    continue;
                }
            }

            return false;
        }

        public ICollection<IShape> HitTest(Rect rect)
        {
            var hs = new HashSet<IShape>();

            HitTest(Layers.Pins.Shapes.Cast<XPin>(), rect, hs);

            HitTest(Layers.Wires.Shapes.Cast<XWire>(), rect, hs);

            HitTest(Layers.Blocks.Shapes.Cast<XBlock>(), rect, hs);

            HitTest(Layers.Template.Shapes, rect, hs);

            return hs;
        }

        #endregion

        #region Selection Mode

        public ICollection<IShape> GetAllShapes()
        {
            var all = Layers.Pins.Shapes
                .Concat(Layers.Wires.Shapes)
                .Concat(Layers.Blocks.Shapes)
                .Concat(Layers.Template.Shapes);
            return new HashSet<IShape>(all);
        }

        public void SelectAll()
        {
            var hs = GetAllShapes();
            if (hs != null && hs.Count > 0)
            {
                Renderer.Selected = hs;
                InvalidatePage();
            }
        }

        public void SelectionDelete()
        {
            if (Renderer.Selected != null
                && Renderer.Selected.Count > 0)
            {
                if (History != null)
                {
                    History.Snapshot(Store("Page"));
                }
                Delete(Renderer.Selected);
                SelectionReset();
            }
        }

        public void SelectionReset()
        {
            if (Renderer.Selected != null)
            {
                Renderer.Selected = null;
                InvalidatePage();
            }
        }

        private void SelectionInit(Point p)
        {
            IShape shape = Layers != null ? HitTest(p) : null;
            if (shape != null)
            {
                MoveInit(shape, p);
            }
            else
            {
                SelectionReset();
                SelectionStart(p);
            }
        }

        private void SelectionStart(Point p)
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
            ReleaseMouseCapture();
            Shapes.Remove(_selection);
            InvalidateVisual();
            _mode = Mode.None;

            var rect = new Rect(
                Math.Min(_startx, p.X),
                Math.Min(_starty, p.Y),
                Math.Abs(p.X - _startx),
                Math.Abs(p.Y - _starty));

            var hs = HitTest(rect);
            if (hs != null && hs.Count > 0)
            {
                Renderer.Selected = hs;
            }
            else
            {
                Renderer.Selected = null;
            }
            InvalidatePage();
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
            _startx = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            _starty = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            if (Renderer.Selected != null)
            {
                _element = Element.Selected;
            }
            else
            {
                MoveInitElement(shape);
            }

            CaptureMouse();
            _mode = Mode.Move;
        }

        private void MoveInitElement(IShape shape)
        {
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
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            double dx = x - _startx;
            double dy = y - _starty;

            _startx = x;
            _starty = y;

            if (Renderer.Selected != null)
            {
                MoveSelected(dx, dy);
            }
            else
            {
                MoveElement(dx, dy);
            }
        }

        private void MoveElement(double dx, double dy)
        {
            switch (_element)
            {
                case Element.Line:
                    switch (_lineHit)
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
                    Layers.Wires.InvalidateVisual();
                    Layers.Pins.InvalidateVisual();
                    break;
                case Element.Block:
                    XCanvas.Move(_block, dx, dy);
                    Layers.Blocks.InvalidateVisual();
                    Layers.Wires.InvalidateVisual();
                    Layers.Pins.InvalidateVisual();
                    break;
            }
        }

        private void MoveSelected(double dx, double dy)
        {
            foreach(var shape in Renderer.Selected)
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
                    // TODO:
                }
                else if (shape is XPin)
                {
                    var pin = shape as XPin;
                    pin.X += dx;
                    pin.Y += dy;
                }
                else if (shape is XBlock)
                {
                    var block = shape as XBlock;
                    XCanvas.Move(block, dx, dy);
                }
            }

            Layers.Template.InvalidateVisual();
            Layers.Blocks.InvalidateVisual();
            Layers.Wires.InvalidateVisual();
            Layers.Pins.InvalidateVisual();
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
            IShape pinHitResult = null;
            IShape wireHitResult = null;
            IShape blockHitResult = null;

            if (Layers != null)
            {
                pinHitResult = HitTest(Layers.Pins.Shapes.Cast<XPin>(), p);
                if (pinHitResult == null)
                {
                    wireHitResult = HitTest(Layers.Wires.Shapes.Cast<XWire>(), p);
                    if (wireHitResult == null)
                    {
                        blockHitResult = HitTest(Layers.Blocks.Shapes.Cast<XBlock>(), p);
                    }
                }
            }

            CreateInit(p);

            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            if (pinHitResult == null
                && wireHitResult == null
                && (blockHitResult == null || (blockHitResult != null && !(blockHitResult is XPin))))
            {
                if (Layers.Pins != null)
                {
                    // create new standalone pin
                    pinHitResult = new XPin()
                    {
                        X = x,
                        Y = y,
                    };

                    _pin = pinHitResult as XPin;

                    Shapes.Add(_pin);
                    InvalidateVisual();
                }
            }

            if (pinHitResult != null
                || wireHitResult != null
                || (blockHitResult != null && blockHitResult is XPin))
            {
                // connect wire start
                if (pinHitResult != null)
                {
                    _wire.Start = pinHitResult as XPin;
                }
                else if (wireHitResult != null && wireHitResult is XWire)
                {
                    // split wire
                    if (Layers.Pins != null && Layers.Wires != null)
                    {
                        SplitStart(wireHitResult, x, y);
                    }
                }
                else if (blockHitResult != null && blockHitResult is XPin)
                {
                    _wire.Start = blockHitResult as XPin;
                }
            }
        }

        private void CreateWireFinish(Point p)
        {
            IShape pinHitResult = null;
            IShape wireHitResult = null;
            IShape blockHitResult = null;

            if (Layers != null)
            {
                pinHitResult = HitTest(Layers.Pins.Shapes.Cast<XPin>(), p);
                if (pinHitResult == null)
                {
                    wireHitResult = HitTest(Layers.Wires.Shapes.Cast<XWire>(), p);
                    if (wireHitResult == null)
                    {
                        blockHitResult = HitTest(Layers.Blocks.Shapes.Cast<XBlock>(), p);
                    }
                }
            }

            CreateFinish(p);

            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            if (pinHitResult == null
                && wireHitResult == null
                && (blockHitResult == null || (blockHitResult != null && !(blockHitResult is XPin))))
            {
                if (Layers.Pins != null)
                {
                    // create new standalone pin
                    pinHitResult = new XPin()
                    {
                        X = x,
                        Y = y,
                    };

                    Layers.Pins.Shapes.Add(pinHitResult);
                    Layers.Pins.InvalidateVisual();
                }
            }

            // connect wire end
            if (pinHitResult != null)
            {
                _wire.End = pinHitResult as XPin;
            }
            else if (wireHitResult != null && wireHitResult is XWire)
            {
                // split wire
                if (Layers.Pins != null && Layers.Wires != null)
                {
                    SplitEnd(wireHitResult, x, y);
                }
            }
            else if (blockHitResult != null && blockHitResult is XPin)
            {
                _wire.End = blockHitResult as XPin;
            }
        }

        private void CreateInit(Point p)
        {
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

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
                        _pin = null;
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
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

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
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

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

                            if (_pin != null)
                            {
                                Shapes.Remove(_pin);
                            }

                            if (History != null)
                            {
                                History.Snapshot(Store("Page"));
                            }

                            Layers.Wires.Shapes.Add(_wire);
                            Layers.Wires.InvalidateVisual();

                            if (_pin != null)
                            {
                                Layers.Pins.Shapes.Add(_pin);
                                Layers.Pins.InvalidateVisual();
                            }
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
                        if (_pin != null)
                        {
                            Shapes.Remove(_pin);
                        }
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

        #region Wire

        private void Split(XWire wire, double x, double y, out XPin pin, out XWire split)
        {
            pin = new XPin()
            {
                X = x,
                Y = y,
            };

            split = new XWire()
            {
                Start = pin as XPin,
                End = wire.End,
                InvertStart = false,
                InvertEnd = wire.InvertEnd
            };

            wire.InvertEnd = false;
            wire.End = pin as XPin;
        }

        private void SplitStart(IShape wireHitResult, double x, double y)
        {
            XPin pin;
            XWire split;

            Split(wireHitResult as XWire, x, y, out pin, out split);

            _wire.Start = pin;

            Layers.Pins.Shapes.Add(pin);
            Layers.Wires.Shapes.Add(split);

            Layers.Wires.InvalidateVisual();
            Layers.Pins.InvalidateVisual();
        }

        private void SplitEnd(IShape wireHitResult, double x, double y)
        {
            XPin pin;
            XWire split;

            Split(wireHitResult as XWire, x, y, out pin, out split);

            _wire.End = pin;

            Layers.Pins.Shapes.Add(pin);
            Layers.Wires.Shapes.Add(split);

            Layers.Wires.InvalidateVisual();
            Layers.Pins.InvalidateVisual();
        }

        #endregion

        #region Render

        public void InvalidatePage()
        {
            if (Layers != null)
            {
                Layers.Template.InvalidateVisual();
                Layers.Blocks.InvalidateVisual();
                Layers.Pins.InvalidateVisual();
                Layers.Wires.InvalidateVisual(); 
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (Renderer != null)
            {
                if (_mode == Mode.Selection)
                {
                    Renderer.DrawSelection(dc, _selectionStyle, _selection);
                }
                else
                {
                    Renderer.DrawShapes(dc, _shapeStyle, _selectedShapeStyle, Shapes);
                }
            }
        }

        #endregion
    }
}
