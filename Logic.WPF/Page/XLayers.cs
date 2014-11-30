using Logic.Core;
using Logic.Util;
using Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Page
{
    public class XLayers
    {
        #region Enums

        public enum LineHit
        {
            None,
            Start,
            End,
            Line
        }

        #endregion

        #region Properties

        public MainViewModel Model { get; set; }
        public ToolMenuModel Tool { get; set; }

        public XLayer Shapes { get; set; }
        public XLayer Blocks { get; set; }
        public XLayer Wires { get; set; }
        public XLayer Pins { get; set; }
        public XLayer Editor { get; set; }
        public XLayer Overlay { get; set; }

        public LineHit LineHitResult { get; set; }
        public IRenderer Renderer { get; set; }

        #endregion

        #region Page

        public IPage ToPage(string name, ITemplate template)
        {
            return new XPage()
            {
                Name = Model.Page.Name,
                Shapes = Model.Page.Shapes,
                Blocks = Model.Page.Blocks,
                Pins = Model.Page.Pins,
                Wires = Model.Page.Wires,
                Template = null
            };
        } 

        public void Load(IPage page)
        {
            Shapes.Shapes = page.Shapes;
            Blocks.Shapes = page.Blocks;
            Wires.Shapes = page.Wires;
            Pins.Shapes = page.Pins;

            Editor.Shapes.Clear();
            Overlay.Shapes.Clear();
        }

        public void Update(IPage page)
        {
            Model.Page.Shapes = page.Shapes;
            Model.Page.Blocks = page.Blocks;
            Model.Page.Wires = page.Wires;
            Model.Page.Pins = page.Pins;
        }

        public void Reset()
        {
            Shapes.Shapes = Enumerable.Empty<IShape>().ToList();
            Blocks.Shapes = Enumerable.Empty<IShape>().ToList();
            Wires.Shapes = Enumerable.Empty<IShape>().ToList();
            Pins.Shapes = Enumerable.Empty<IShape>().ToList();
        }

        #endregion

        #region Add

        public void Add(IEnumerable<IShape> shapes)
        {
            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    Shapes.Shapes.Add(shape);
                }
                else if (shape is XEllipse)
                {
                    Shapes.Shapes.Add(shape);
                }
                else if (shape is XRectangle)
                {
                    Shapes.Shapes.Add(shape);
                }
                else if (shape is XText)
                {
                    Shapes.Shapes.Add(shape);
                }
                else if (shape is XWire)
                {
                    Wires.Shapes.Add(shape);
                }
                else if (shape is XPin)
                {
                    Pins.Shapes.Add(shape);
                }
                else if (shape is XBlock)
                {
                    Blocks.Shapes.Add(shape);
                }
            }
        } 

        #endregion

        #region Delete

        public void Delete(IEnumerable<IShape> shapes)
        {
            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    Shapes.Shapes.Remove(shape);
                }
                else if (shape is XEllipse)
                {
                    Shapes.Shapes.Remove(shape);
                }
                else if (shape is XRectangle)
                {
                    Shapes.Shapes.Remove(shape);
                }
                else if (shape is XText)
                {
                    Shapes.Shapes.Remove(shape);
                }
                else if (shape is XWire)
                {
                    Wires.Shapes.Remove(shape);
                }
                else if (shape is XPin)
                {
                    Pins.Shapes.Remove(shape);
                }
                else if (shape is XBlock)
                {
                    Blocks.Shapes.Remove(shape);
                }
            }
        } 
        
        #endregion

        #region GetAll

        public ICollection<IShape> GetAll()
        {
            return 
                new HashSet<IShape>(
                    Enumerable.Empty<IShape>()
                              .Concat(Pins.Shapes)
                              .Concat(Wires.Shapes)
                              .Concat(Blocks.Shapes)
                              .Concat(Shapes.Shapes));
        }

        #endregion

        #region SelectAll

        public void SelectAll()
        {
            var shapes = GetAll();
            if (shapes != null && shapes.Count > 0)
            {
                Renderer.Selected = shapes;
            }
        }

        #endregion

        #region Math

        private bool LineIntersectsWithRect(
            double left, double right,
            double bottom, double top,
            double x0, double y0,
            double x1, double y1)
        {
            // Liang-Barsky line clipping algorithm
            double t0 = 0.0;
            double t1 = 1.0;
            double dx = x1 - x0;
            double dy = y1 - y0;
            double p = 0.0, q = 0.0, r;

            for (int edge = 0; edge < 4; edge++)
            {
                if (edge == 0)
                {
                    p = -dx;
                    q = -(left - x0);
                }
                if (edge == 1)
                {
                    p = dx;
                    q = (right - x0);
                }
                if (edge == 2)
                {
                    p = dy;
                    q = (bottom - y0);
                }
                if (edge == 3)
                {
                    p = -dy;
                    q = -(top - y0);
                }

                r = q / p;

                if (p == 0.0 && q < 0.0)
                {
                    return false;
                }

                if (p < 0.0)
                {
                    if (r > t1)
                    {
                        return false;
                    }
                    else if (r > t0)
                    {
                        t0 = r;
                    }
                }
                else if (p > 0.0)
                {
                    if (r < t0)
                    {
                        return false;
                    }
                    else if (r < t1)
                    {
                        t1 = r;
                    }
                }
            }

            // Clipped line
            //double x0clip = x0 + t0 * dx;
            //double y0clip = y0 + t0 * dy;
            //double x1clip = x0 + t1 * dx;
            //double y1clip = y0 + t1 * dy;

            return true;
        }

        private Point2 NearestPointOnLine(Point2 a, Point2 b, Point2 p)
        {
            double ax = p.X - a.X;
            double ay = p.Y - a.Y;
            double bx = b.X - a.X;
            double by = b.Y - a.Y;
            double t = (ax * bx + ay * by) / (bx * bx + by * by);
            if (t < 0.0)
            {
                return new Point2(a.X, a.Y);
            }
            else if (t > 1.0)
            {
                return new Point2(b.X, b.Y);
            }
            return new Point2(bx * t + a.X, by * t + a.Y);
        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void Middle(ref Point2 point, double x1, double y1, double x2, double y2)
        {
            point.X = (x1 + x2) / 2.0;
            point.Y = (y1 + y2) / 2.0;
        }

        #endregion

        #region Bounds

        private Rect2 GetPinBounds(double x, double y)
        {
            return new Rect2(
                x - Renderer.PinRadius,
                y - Renderer.PinRadius,
                Renderer.PinRadius + Renderer.PinRadius,
                Renderer.PinRadius + Renderer.PinRadius);
        }

        private Rect2 GetEllipseBounds(XEllipse ellipse)
        {
            var bounds = new Rect2(
                ellipse.X - ellipse.RadiusX,
                ellipse.Y - ellipse.RadiusY,
                ellipse.RadiusX + ellipse.RadiusX,
                ellipse.RadiusY + ellipse.RadiusY);
            return bounds;
        }

        private Rect2 GetRectangleBounds(XRectangle rectangle)
        {
            var bounds = new Rect2(
                rectangle.X,
                rectangle.Y,
                rectangle.Width,
                rectangle.Height);
            return bounds;
        }

        private Rect2 GetTextBounds(XText text)
        {
            var bounds = new Rect2(
                text.X,
                text.Y,
                text.Width,
                text.Height);
            return bounds;
        }

        #endregion

        #region HitTest

        public bool HitTest(XLine line, Point2 p, double treshold)
        {
            var a = new Point2(line.X1, line.Y1);
            var b = new Point2(line.X2, line.Y2);
            var nearest = NearestPointOnLine(a, b, p);
            double distance = Distance(p.X, p.Y, nearest.X, nearest.Y);
            return distance < treshold;
        }

        public bool HitTest(XWire wire, Point2 p, double treshold)
        {
            var a = wire.Start != null ?
                new Point2(wire.Start.X, wire.Start.Y) : new Point2(wire.X1, wire.Y1);
            var b = wire.End != null ?
                new Point2(wire.End.X, wire.End.Y) : new Point2(wire.X2, wire.Y2);
            var nearest = NearestPointOnLine(a, b, p);
            double distance = Distance(p.X, p.Y, nearest.X, nearest.Y);
            return distance < treshold;
        }

        public IShape HitTest(IEnumerable<XPin> pins, Point2 p)
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

        public IShape HitTest(IEnumerable<XWire> wires, Point2 p)
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

                if (HitTest(wire, p, Renderer.HitTreshold))
                {
                    return wire;
                }
            }

            return null;
        }

        public IShape HitTest(IEnumerable<XBlock> blocks, Point2 p)
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

        public IShape HitTest(IEnumerable<IShape> shapes, Point2 p)
        {
            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    var line = shape as XLine;

                    if (GetPinBounds(line.X1, line.Y1).Contains(p))
                    {
                        LineHitResult = LineHit.Start;
                        return line;
                    }

                    if (GetPinBounds(line.X2, line.Y2).Contains(p))
                    {
                        LineHitResult = LineHit.End;
                        return line;
                    }

                    if (HitTest(line, p, Renderer.HitTreshold))
                    {
                        LineHitResult = LineHit.Line;
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

        public IShape HitTest(Point2 p)
        {
            var pin = HitTest(Pins.Shapes.Cast<XPin>(), p);
            if (pin != null)
            {
                return pin;
            }

            var wire = HitTest(Wires.Shapes.Cast<XWire>(), p);
            if (wire != null)
            {
                return wire;
            }

            var block = HitTest(Blocks.Shapes.Cast<XBlock>(), p);
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

            var template = HitTest(Shapes.Shapes, p);
            if (template != null)
            {
                return template;
            }

            return null;
        }

        public bool HitTest(IEnumerable<XPin> pins, Rect2 rect, ICollection<IShape> hs)
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

        public bool HitTest(IEnumerable<XWire> wires, Rect2 rect, ICollection<IShape> hs)
        {
            foreach (var wire in wires)
            {
                double sx, sy, ex, ey;
                if (wire.Start != null)
                {
                    sx = wire.Start.X;
                    sy = wire.Start.Y;
                }
                else
                {
                    sx = wire.X1;
                    sy = wire.Y1;
                }

                if (wire.End != null)
                {
                    ex = wire.End.X;
                    ey = wire.End.Y;
                }
                else
                {
                    ex = wire.X2;
                    ey = wire.Y2;
                }

                if (GetPinBounds(sx, sy).IntersectsWith(rect)
                    || GetPinBounds(ex, ey).IntersectsWith(rect)
                    || LineIntersectsWithRect(rect.Left, rect.Right, rect.Bottom, rect.Top, sx, sy, ex, ey))
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

            return false;
        }

        public bool HitTest(IEnumerable<XBlock> blocks, Rect2 rect, ICollection<IShape> hs)
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

        public bool HitTest(IEnumerable<IShape> shapes, Rect2 rect, ICollection<IShape> hs)
        {
            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    var line = shape as XLine;
                    if (GetPinBounds(line.X1, line.Y1).IntersectsWith(rect)
                        || GetPinBounds(line.X2, line.Y2).IntersectsWith(rect)
                        || LineIntersectsWithRect(rect.Left, rect.Right, rect.Bottom, rect.Top, line.X1, line.Y1, line.X2, line.Y2))
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

        public ICollection<IShape> HitTest(Rect2 rect)
        {
            var hs = new HashSet<IShape>();
            HitTest(Pins.Shapes.Cast<XPin>(), rect, hs);
            HitTest(Wires.Shapes.Cast<XWire>(), rect, hs);
            HitTest(Blocks.Shapes.Cast<XBlock>(), rect, hs);
            HitTest(Shapes.Shapes, rect, hs);
            return hs;
        }

        #endregion

        #region Invalidate

        public void Invalidate()
        {
            Shapes.InvalidateVisual();
            Blocks.InvalidateVisual();
            Pins.InvalidateVisual();
            Wires.InvalidateVisual();
            Overlay.InvalidateVisual();
        }

        #endregion
    }
}
