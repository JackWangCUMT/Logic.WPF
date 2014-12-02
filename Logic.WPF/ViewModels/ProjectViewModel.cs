using Logic.Core;
using Logic.Page;
using Logic.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Logic.ViewModels
{
    public class ProjectViewModel : NotifyObject
    {
        #region Properties

        private IProject _project;
        public IProject Project
        {
            get { return _project; }
            set
            {
                if (value != _project)
                {
                    _project = value;
                    Notify("Project");
                }
            }
        }

        private IPage _page;
        public IPage Page
        {
            get { return _page; }
            set
            {
                if (value != _page)
                {
                    _page = value;
                    Notify("Page");
                }
            }
        }

        private IList<XBlock> _blocks;

        [ImportMany(typeof(XBlock))]
        public IList<XBlock> Blocks
        {
            get { return _blocks; }
            set
            {
                if (value != _blocks)
                {
                    _blocks = value;
                    Notify("Blocks");
                }
            }
        }

        private IList<ITemplate> _templates;

        [ImportMany(typeof(ITemplate))]
        public IList<ITemplate> Templates
        {
            get { return _templates; }
            set
            {
                if (value != _templates)
                {
                    _templates = value;
                    Notify("Templates");
                }
            }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                    Notify("FileName");
                }
            }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (value != _filePath)
                {
                    _filePath = value;
                    Notify("FilePath");
                }
            }
        }

        private ToolMenuModel _tool;
        public ToolMenuModel Tool
        {
            get { return _tool; }
            set
            {
                if (value != _tool)
                {
                    _tool = value;
                    Notify("Tool");
                }
            }
        }

        private IShape _selected;
        public IShape Selected
        {
            get { return _selected; }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    Notify("Selected");
                }
            }
        }

        private bool _haveSelected;
        public bool HaveSelected
        {
            get { return _haveSelected; }
            set
            {
                if (value != _haveSelected)
                {
                    _haveSelected = value;
                    Notify("HaveSelected");
                }
            }
        }

        public LayerViewModel ShapeLayer { get; set; }
        public LayerViewModel BlockLayer { get; set; }
        public LayerViewModel WireLayer { get; set; }
        public LayerViewModel PinLayer { get; set; }
        public LayerViewModel EditorLayer { get; set; }
        public LayerViewModel OverlayLayer { get; set; }

        public LineHit LineHitResult { get; set; }
        public IRenderer Renderer { get; set; }

        public History<IPage> History { get; set; }
        public ITextClipboard Clipboard { get; set; }
        public IStringSerializer Serializer { get; set; }

        #endregion

        #region Commands

        public ICommand SelectedItemChangedCommand { get; set; }

        public ICommand PageAddCommand { get; set; }
        public ICommand PageInsertBeforeCommand { get; set; }
        public ICommand PageInsertAfterCommand { get; set; }
        public ICommand PageCutCommand { get; set; }
        public ICommand PageCopyCommand { get; set; }
        public ICommand PagePasteCommand { get; set; }
        public ICommand PageDeleteCommand { get; set; }

        public ICommand DocumentAddCommand { get; set; }
        public ICommand DocumentInsertBeforeCommand { get; set; }
        public ICommand DocumentInsertAfterCommand { get; set; }
        public ICommand DocumentCutCommand { get; set; }
        public ICommand DocumentCopyCommand { get; set; }
        public ICommand DocumentPasteCommand { get; set; }
        public ICommand DocumentDeleteCommand { get; set; }

        public ICommand FileNewCommand { get; set; }
        public ICommand FileOpenCommand { get; set; }
        public ICommand FileSaveCommand { get; set; }
        public ICommand FileSaveAsCommand { get; set; }
        public ICommand FileSaveAsPDFCommand { get; set; }
        public ICommand FileExitCommand { get; set; }

        public ICommand EditUndoCommand { get; set; }
        public ICommand EditRedoCommand { get; set; }
        public ICommand EditCutCommand { get; set; }
        public ICommand EditCopyCommand { get; set; }
        public ICommand EditPasteCommand { get; set; }
        public ICommand EditDeleteCommand { get; set; }
        public ICommand EditSelectAllCommand { get; set; }

        public ICommand EditAlignLeftBottomCommand { get; set; }
        public ICommand EditAlignBottomCommand { get; set; }
        public ICommand EditAlignRightBottomCommand { get; set; }
        public ICommand EditAlignLeftCommand { get; set; }
        public ICommand EditAlignCenterCenterCommand { get; set; }
        public ICommand EditAlignRightCommand { get; set; }
        public ICommand EditAlignLeftTopCommand { get; set; }
        public ICommand EditAlignTopCommand { get; set; }
        public ICommand EditAlignRightTopCommand { get; set; }

        public ICommand EditIncreaseTextSizeCommand { get; set; }
        public ICommand EditDecreaseTextSizeCommand { get; set; }
        public ICommand EditToggleFillCommand { get; set; }
        public ICommand EditToggleSnapCommand { get; set; }
        public ICommand EditToggleInvertStartCommand { get; set; }
        public ICommand EditToggleInvertEndCommand { get; set; }

        public ICommand EditCancelCommand { get; set; }

        public ICommand ToolNoneCommand { get; set; }
        public ICommand ToolSelectionCommand { get; set; }
        public ICommand ToolWireCommand { get; set; }
        public ICommand ToolPinCommand { get; set; }
        public ICommand ToolLineCommand { get; set; }
        public ICommand ToolEllipseCommand { get; set; }
        public ICommand ToolRectangleCommand { get; set; }
        public ICommand ToolTextCommand { get; set; }
        public ICommand ToolImageCommand { get; set; }

        public ICommand BlockImportCommand { get; set; }
        public ICommand BlockImportCodeCommand { get; set; }
        public ICommand BlockExportCommand { get; set; }
        public ICommand BlockCreateProjectCommand { get; set; }
        public ICommand InsertBlockCommand { get; set; }

        public ICommand TemplateImportCommand { get; set; }
        public ICommand TemplateImportCodeCommand { get; set; }
        public ICommand TemplateExportCommand { get; set; }
        public ICommand ApplyTemplateCommand { get; set; }

        public ICommand SimulationStartCommand { get; set; }
        public ICommand SimulationStopCommand { get; set; }
        public ICommand SimulationRestartCommand { get; set; }
        public ICommand SimulationCreateGraphCommand { get; set; }
        public ICommand SimulationOptionsCommand { get; set; }

        #endregion

        #region Enums

        public enum LineHit
        {
            None,
            Start,
            End,
            Line
        }

        #endregion

        #region Add

        public void Add(IEnumerable<IShape> shapes)
        {
            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    ShapeLayer.Shapes.Add(shape);
                }
                else if (shape is XEllipse)
                {
                    ShapeLayer.Shapes.Add(shape);
                }
                else if (shape is XRectangle)
                {
                    ShapeLayer.Shapes.Add(shape);
                }
                else if (shape is XText)
                {
                    ShapeLayer.Shapes.Add(shape);
                }
                else if (shape is XImage)
                {
                    ShapeLayer.Shapes.Add(shape);
                }
                else if (shape is XWire)
                {
                    WireLayer.Shapes.Add(shape);
                }
                else if (shape is XPin)
                {
                    PinLayer.Shapes.Add(shape);
                }
                else if (shape is XBlock)
                {
                    BlockLayer.Shapes.Add(shape);
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
                    ShapeLayer.Shapes.Remove(shape);
                }
                else if (shape is XEllipse)
                {
                    ShapeLayer.Shapes.Remove(shape);
                }
                else if (shape is XRectangle)
                {
                    ShapeLayer.Shapes.Remove(shape);
                }
                else if (shape is XText)
                {
                    ShapeLayer.Shapes.Remove(shape);
                }
                else if (shape is XImage)
                {
                    ShapeLayer.Shapes.Remove(shape);
                }
                else if (shape is XWire)
                {
                    WireLayer.Shapes.Remove(shape);
                }
                else if (shape is XPin)
                {
                    PinLayer.Shapes.Remove(shape);
                }
                else if (shape is XBlock)
                {
                    BlockLayer.Shapes.Remove(shape);
                }
            }
        }

        #endregion

        #region Clone

        public XBlock Clone(XBlock original)
        {
            try
            {
                var block = new XBlock()
                {
                    Properties = original.Properties,
                    Database = original.Database,
                    Name = original.Name,
                    Style = original.Style,
                    Shapes = original.Shapes,
                    Pins = original.Pins
                };
                var json = Serializer.Serialize(block);
                var copy = Serializer.Deserialize<XBlock>(json);
                foreach (var pin in copy.Pins)
                {
                    pin.Owner = copy;
                }
                return copy;
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        public XTemplate Clone(ITemplate original)
        {
            try
            {
                var template = new XTemplate()
                {
                    Database = original.Database,
                    Width = original.Width,
                    Height = original.Height,
                    Name = original.Name,
                    Grid = original.Grid,
                    Table = original.Table,
                    Frame = original.Frame
                };
                var json = Serializer.Serialize(template);
                var copy = Serializer.Deserialize<XTemplate>(json);
                return copy;
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        #endregion

        #region GetAll

        public ICollection<IShape> GetAll()
        {
            return
                new HashSet<IShape>(
                    Enumerable.Empty<IShape>()
                              .Concat(PinLayer.Shapes)
                              .Concat(WireLayer.Shapes)
                              .Concat(BlockLayer.Shapes)
                              .Concat(ShapeLayer.Shapes));
        }

        #endregion

        #region Selection

        public void SelectAll()
        {
            var shapes = GetAll();
            if (shapes != null && shapes.Count > 0)
            {
                Renderer.Selected = shapes;
            }
        }

        public bool HaveSelection()
        {
            return Renderer != null
                && Renderer.Selected != null
                && Renderer.Selected.Count > 0;
        }

        public void SelectionDelete()
        {
            if (HaveSelection())
            {
                Snapshot();
                Delete(Renderer.Selected);
                SelectionReset();
            }
        }

        public void SelectionReset()
        {
            if (Renderer != null
                && Renderer.Selected != null)
            {
                Renderer.Selected = null;
                Invalidate();
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

        private Rect2 GetImageBounds(XImage image)
        {
            var bounds = new Rect2(
                image.X,
                image.Y,
                image.Width,
                image.Height);
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
                else if (shape is XImage)
                {
                    if (GetImageBounds(shape as XImage).Contains(p))
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
            var pin = HitTest(PinLayer.Shapes.Cast<XPin>(), p);
            if (pin != null)
            {
                return pin;
            }

            var wire = HitTest(WireLayer.Shapes.Cast<XWire>(), p);
            if (wire != null)
            {
                return wire;
            }

            var block = HitTest(BlockLayer.Shapes.Cast<XBlock>(), p);
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

            var template = HitTest(ShapeLayer.Shapes, p);
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
                else if (shape is XImage)
                {
                    if (GetImageBounds(shape as XImage).IntersectsWith(rect))
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
            HitTest(PinLayer.Shapes.Cast<XPin>(), rect, hs);
            HitTest(WireLayer.Shapes.Cast<XWire>(), rect, hs);
            HitTest(BlockLayer.Shapes.Cast<XBlock>(), rect, hs);
            HitTest(ShapeLayer.Shapes, rect, hs);
            return hs;
        }

        #endregion

        #region History

        public void Snapshot()
        {
            History.Snapshot(ToPage());
        }

        public void Reset()
        {
            History.Reset();
        }

        public void Hold()
        {
            History.Hold(ToPage());
        }

        public void Commit()
        {
            History.Commit();
        }

        public void Release()
        {
            History.Release();
        }

        public void Undo()
        {
            var page = History.Undo(ToPage());
            if (page != null)
            {
                SelectionReset();
                Load(page);
                Update(page);
                Invalidate();
            }
        }

        public void Redo()
        {
            var page = History.Redo(ToPage());
            if (page != null)
            {
                SelectionReset();
                Load(page);
                Update(page);
                Invalidate();
            }
        }

        #endregion

        #region Clipboard

        private void CopyToClipboard(IList<IShape> shapes)
        {
            try
            {
                var json = Serializer.Serialize(shapes);
                if (!string.IsNullOrEmpty(json))
                {
                    Clipboard.SetText(json);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        public bool CanCopy()
        {
            return HaveSelection();
        }

        public bool CanPaste()
        {
            try
            {
                return Clipboard.ContainsText();
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return false;
        }

        public void Cut()
        {
            if (CanCopy())
            {
                CopyToClipboard(Renderer.Selected.ToList());
                SelectionDelete();
            }
        }

        public void Copy()
        {
            if (CanCopy())
            {
                CopyToClipboard(Renderer.Selected.ToList());
            }
        }

        public void Paste()
        {
            try
            {
                if (CanPaste())
                {
                    var json = Clipboard.GetText();
                    if (!string.IsNullOrEmpty(json))
                    {
                        Paste(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        public void Paste(string json)
        {
            try
            {
                var shapes = Serializer.Deserialize<IList<IShape>>(json);
                if (shapes != null && shapes.Count > 0)
                {
                    Paste(shapes);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        public void Paste(IEnumerable<IShape> shapes)
        {
            Snapshot();
            SelectionReset();
            Add(shapes);
            Renderer.Selected = new HashSet<IShape>(shapes);
            Invalidate();
        }

        #endregion

        #region Serialization

        public T Open<T>(string path) where T : class
        {
            try
            {
                using (var fs = System.IO.File.OpenText(path))
                {
                    var json = fs.ReadToEnd();
                    var project = Serializer.Deserialize<T>(json);
                    return project;
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        public void Save<T>(string path, T project) where T : class
        {
            try
            {
                var json = Serializer.Serialize<T>(project);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        #endregion

        #region Page

        public IPage ToPage()
        {
            return new XPage()
            {
                Name = Page.Name,
                Shapes = Page.Shapes,
                Blocks = Page.Blocks,
                Pins = Page.Pins,
                Wires = Page.Wires,
                Template = null
            };
        }

        public void Load(IPage page)
        {
            ShapeLayer.Shapes = page.Shapes;
            BlockLayer.Shapes = page.Blocks;
            WireLayer.Shapes = page.Wires;
            PinLayer.Shapes = page.Pins;

            EditorLayer.Shapes.Clear();
            OverlayLayer.Shapes.Clear();
        }

        public void Update(IPage page)
        {
            Page.Shapes = page.Shapes;
            Page.Blocks = page.Blocks;
            Page.Wires = page.Wires;
            Page.Pins = page.Pins;
        }

        public void Clear()
        {
            ShapeLayer.Shapes = Enumerable.Empty<IShape>().ToList();
            BlockLayer.Shapes = Enumerable.Empty<IShape>().ToList();
            WireLayer.Shapes = Enumerable.Empty<IShape>().ToList();
            PinLayer.Shapes = Enumerable.Empty<IShape>().ToList();
        }

        #endregion

        #region Project

        public IProject Load(string path)
        {
            var project = Open<XProject>(path);
            if (project != null)
            {
                SelectionReset();
                Reset();
                Invalidate();
            }
            return project;
        }

        #endregion

        #region Invalidate

        public void Invalidate()
        {
            ShapeLayer.InvalidateVisual();
            BlockLayer.InvalidateVisual();
            PinLayer.InvalidateVisual();
            WireLayer.InvalidateVisual();
            OverlayLayer.InvalidateVisual();
        }

        #endregion
    }
}
