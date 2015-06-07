// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.Simulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.ViewModels
{
    public class LayerViewModel : ILayer
    {
        #region Enums

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
            Image,
            Wire,
            Pin,
            Block
        }

        #endregion

        #region Properties

        public Func<string> GetFilePath { get; set; }

        public IList<IShape> Shapes { get; set; }
        public ICollection<IShape> Hidden { get; set; }
        public IRenderer Renderer { get; set; }
        public IStyle ShapeStyle { get; set; }
        public IStyle SelectedShapeStyle { get; set; }
        public IStyle SelectionStyle { get; set; }
        public IStyle HoverStyle { get; set; }
        public IStyle NullStateStyle { get; set; }
        public IStyle TrueStateStyle { get; set; }
        public IStyle FalseStateStyle { get; set; }

        public MainViewModel Layers { get; set; }
        public IDictionary<XBlock, BoolSimulation> Simulations { get; set; }
        public bool EnableSnap { get; set; }
        public double SnapSize { get; set; }
        public bool IsOverlay { get; set; }
        public Mode CurrentMode { get { return _mode; } }
        public bool SkipContextMenu { get; set; }
        public double RightX { get; set; }
        public double RightY { get; set; }
        public IBoolSimulationRenderer CacheRenderer { get; set; }
        public bool EnableSimulationCache { get; set; }

        #endregion

        #region Fields

        private double _startx, _starty;
        private double _hx, _hy;
        private XBlock _block = null;
        private XLine _line = null;
        private XEllipse _ellipse = null;
        private XRectangle _rectangle = null;
        private XText _text = null;
        private XImage _image = null;
        private XWire _wire = null;
        private XPin _pin = null;
        private XRectangle _selection = null;
        private Mode _mode = Mode.None;
        private Element _element = Element.None;

        #endregion

        #region ILayer

        public Func<bool> IsMouseCaptured { get; set; }
        public Action CaptureMouse { get; set; }
        public Action ReleaseMouseCapture { get; set; }
        public Action InvalidateVisual { get; set; }

        public void MouseLeftButtonDown(Point2 point)
        {
            switch (_mode)
            {
                case Mode.None:
                    if (IsMouseCaptured() == false)
                    {
                        if (Simulations != null && !IsOverlay)
                        {
                            // toggle block state in simulation mode
                            // and do not process other mouse events
                            BlockToggleState(point);
                            return;
                        }

                        switch (Layers.Tool.CurrentTool)
                        {
                            case ToolMenuModel.Tool.None:
                                Layers.SelectionReset();
                                break;
                            case ToolMenuModel.Tool.Selection:
                                SelectionInit(point);
                                break;
                            case ToolMenuModel.Tool.Line:
                            case ToolMenuModel.Tool.Ellipse:
                            case ToolMenuModel.Tool.Rectangle:
                            case ToolMenuModel.Tool.Text:
                            case ToolMenuModel.Tool.Image:
                            case ToolMenuModel.Tool.Pin:
                                Layers.SelectionReset();
                                CreateInit(point);
                                break;
                            case ToolMenuModel.Tool.Wire:
                                Layers.SelectionReset();
                                CreateWireInit(point);
                                break;
                        }
                    }
                    break;
                case Mode.Selection:
                    break;
                case Mode.Create:
                    if (IsMouseCaptured())
                    {
                        switch (Layers.Tool.CurrentTool)
                        {
                            case ToolMenuModel.Tool.None:
                                break;
                            case ToolMenuModel.Tool.Line:
                            case ToolMenuModel.Tool.Ellipse:
                            case ToolMenuModel.Tool.Rectangle:
                            case ToolMenuModel.Tool.Text:
                            case ToolMenuModel.Tool.Image:
                            case ToolMenuModel.Tool.Pin:
                                CreateFinish(point);
                                break;
                            case ToolMenuModel.Tool.Wire:
                                CreateWireFinish(point);
                                break;
                        }
                    }
                    break;
                case Mode.Move:
                    break;
            }
        }

        public void MouseLeftButtonUp(Point2 point)
        {
            if (IsMouseCaptured())
            {
                switch (_mode)
                {
                    case Mode.None:
                        break;
                    case Mode.Selection:
                        SelectionFinish(point);
                        break;
                    case Mode.Create:
                        break;
                    case Mode.Move:
                        MoveFinish(point);
                        break;
                }
            }
        }

        public void MouseMove(Point2 point)
        {
            if (Layers != null
                && Simulations == null)
            {
                OverlayReset();
            }

            if (_mode != Mode.Move
                && _mode != Mode.Selection
                && Layers.Tool.CurrentTool != ToolMenuModel.Tool.None
                && Renderer != null
                && Renderer.Selected == null
                && Simulations == null)
            {
                OverlayMove(point);
            }

            if (IsMouseCaptured())
            {
                switch (_mode)
                {
                    case Mode.None:
                        break;
                    case Mode.Selection:
                        SelectionMove(point);
                        break;
                    case Mode.Create:
                        CreateMove(point);
                        break;
                    case Mode.Move:
                        Move(point);
                        break;
                };
            }
        }

        public void MouseRightButtonDown(Point2 point)
        {
            RightX = point.X;
            RightY = point.Y;

            if (IsMouseCaptured())
            {
                MouseCancel();
            }
        }

        public void MouseCancel()
        {
            if (IsMouseCaptured())
            {
                switch (_mode)
                {
                    case Mode.None:
                        break;
                    case Mode.Selection:
                        SelectionCancel();
                        SkipContextMenu = true;
                        break;
                    case Mode.Create:
                        CreateCancel();
                        SkipContextMenu = true;
                        break;
                    case Mode.Move:
                        MoveCancel();
                        SkipContextMenu = true;
                        break;
                }
            }
            else
            {
                Layers.InvalidateLayers();
            }
        }

        public void OnRender(object dc)
        {
            if (Renderer != null)
            {
                var sw = Stopwatch.StartNew();

                if (_mode == Mode.Selection)
                {
                    _selection.Render(dc, Renderer, SelectionStyle);
                }
                else if (IsOverlay && Simulations != null)
                {
                    if (EnableSimulationCache 
                        && CacheRenderer != null)
                    {
                        CacheRenderer.Render(dc);
                    }
                    else
                    {
                        RenderSimulationMode(dc);
                    }
                }
                else
                {
                    IStyle normal;
                    IStyle selected;
                    if (IsOverlay)
                    {
                        normal = HoverStyle;
                        selected = HoverStyle;
                    }
                    else
                    {
                        normal = ShapeStyle;
                        selected = SelectedShapeStyle;
                    }

                    if (Renderer != null
                        && Renderer.Selected != null)
                    {
                        RenderSelectedMode(dc, normal, selected);
                    }
                    else if (Renderer != null
                        && Renderer.Selected == null
                        && Hidden != null
                        && Hidden.Count > 0)
                    {
                        RenderHiddenMode(dc, normal);
                    }
                    else
                    {
                        RenderNormalMode(dc, normal);
                    }
                }

                sw.Stop();
                if (sw.Elapsed.TotalMilliseconds > (1000.0 / 60.0))
                {
                    Debug.WriteLine("Canvas OnRender: " + sw.Elapsed.TotalMilliseconds + "ms");
                }
            }
        }

        #endregion

        #region Selection Mode

        private void SelectionInit(Point2 p)
        {
            IShape shape = Layers != null ? Layers.HitTest(p) : null;
            if (shape != null)
            {
                MoveInit(shape, p);
            }
            else
            {
                Layers.SelectionReset();
                SelectionStart(p);
            }
        }

        private void SelectionStart(Point2 p)
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

        private void SelectionMove(Point2 p)
        {
            _selection.X = Math.Min(_startx, p.X);
            _selection.Y = Math.Min(_starty, p.Y);
            _selection.Width = Math.Abs(p.X - _startx);
            _selection.Height = Math.Abs(p.Y - _starty);
            InvalidateVisual();
        }

        private void SelectionFinish(Point2 p)
        {
            ReleaseMouseCapture();
            Shapes.Remove(_selection);
            InvalidateVisual();
            _mode = Mode.None;

            var rect = new Rect2(
                Math.Min(_startx, p.X),
                Math.Min(_starty, p.Y),
                Math.Abs(p.X - _startx),
                Math.Abs(p.Y - _starty));

            var hs = Layers.HitTest(rect);
            if (hs != null && hs.Count > 0)
            {
                Renderer.Selected = hs;
            }
            else
            {
                Renderer.Selected = null;
            }
            Layers.InvalidateLayers();
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

        private void MoveInit(IShape shape, Point2 p)
        {
            Layers.Hold();

            _startx = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            _starty = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            _hx = _startx;
            _hy = _starty;

            if (Renderer != null
                && Renderer.Selected != null)
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
            else if (shape is XImage)
            {
                _element = Element.Image;
                _image = shape as XImage;
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

        private void MoveFinish(Point2 p)
        {
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;
            if (_hx != x || _hy != y)
            {
                Layers.Commit();
            }
            else
            {
                Layers.Release();
            }

            ReleaseMouseCapture();
            _mode = Mode.None;
        }

        private void MoveCancel()
        {
            Layers.Release();
            ReleaseMouseCapture();
            _mode = Mode.None;
        }

        public void Move(Point2 p)
        {
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            double dx = x - _startx;
            double dy = y - _starty;

            _startx = x;
            _starty = y;

            if (Renderer != null
                && Renderer.Selected != null)
            {
                MoveSelected(Renderer.Selected, dx, dy);
            }
            else
            {
                MoveElement(dx, dy);
            }
        }

        public void Move(ICollection<IShape> shapes, double dx, double dy)
        {
            double x = EnableSnap ? Snap(dx, SnapSize) : dx;
            double y = EnableSnap ? Snap(dy, SnapSize) : dy;
            MoveSelected(shapes, x, y);
        }

        private void MoveElement(double dx, double dy)
        {
            switch (_element)
            {
                case Element.Line:
                    switch (Layers.LineHitResult)
                    {
                        case MainViewModel.LineHit.Start:
                            _line.X1 += dx;
                            _line.Y1 += dy;
                            break;
                        case MainViewModel.LineHit.End:
                            _line.X2 += dx;
                            _line.Y2 += dy;
                            break;
                        case MainViewModel.LineHit.Line:
                            _line.X1 += dx;
                            _line.Y1 += dy;
                            _line.X2 += dx;
                            _line.Y2 += dy;
                            break;
                    }
                    Layers.ShapeLayer.InvalidateVisual();
                    break;
                case Element.Ellipse:
                    _ellipse.X += dx;
                    _ellipse.Y += dy;
                    Layers.ShapeLayer.InvalidateVisual();
                    break;
                case Element.Rectangle:
                    _rectangle.X += dx;
                    _rectangle.Y += dy;
                    Layers.ShapeLayer.InvalidateVisual();
                    break;
                case Element.Text:
                    _text.X += dx;
                    _text.Y += dy;
                    Layers.ShapeLayer.InvalidateVisual();
                    break;
                case Element.Image:
                    _image.X += dx;
                    _image.Y += dy;
                    Layers.ShapeLayer.InvalidateVisual();
                    break;
                case Element.Wire:
                    // TODO: Implement wire Move
                    break;
                case Element.Pin:
                    _pin.X += dx;
                    _pin.Y += dy;
                    Layers.WireLayer.InvalidateVisual();
                    Layers.PinLayer.InvalidateVisual();
                    break;
                case Element.Block:
                    Move(_block, dx, dy);
                    Layers.BlockLayer.InvalidateVisual();
                    Layers.WireLayer.InvalidateVisual();
                    Layers.PinLayer.InvalidateVisual();
                    break;
            }
        }

        public void MoveSelected(ICollection<IShape> shapes, double dx, double dy)
        {
            foreach (var shape in shapes)
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
                else if (shape is XImage)
                {
                    var image = shape as XImage;
                    image.X += dx;
                    image.Y += dy;
                }
                else if (shape is XWire)
                {
                    var wire = shape as XWire;
                    // TODO: Implement wire Move
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
                    Move(block, dx, dy);
                }
            }

            Layers.ShapeLayer.InvalidateVisual();
            Layers.BlockLayer.InvalidateVisual();
            Layers.WireLayer.InvalidateVisual();
            Layers.PinLayer.InvalidateVisual();
        }

        public void Move(XBlock block, double dx, double dy)
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
                else if (shape is XImage)
                {
                    var image = shape as XImage;
                    image.X += dx;
                    image.Y += dy;
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

        #region Overlay

        private void OverlayMove(Point2 p)
        {
            if (Layers == null)
                return;

            IShape shapeHitResult = null;
            IShape pinHitResult = null;
            IShape wireHitResult = null;
            IShape blockHitResult = null;

            shapeHitResult = Layers.HitTest(p);
            pinHitResult = Layers.HitTest(Layers.PinLayer.Shapes.Cast<XPin>(), p);
            if (pinHitResult == null)
            {
                wireHitResult = Layers.HitTest(Layers.WireLayer.Shapes.Cast<XWire>(), p);
                if (wireHitResult == null)
                {
                    blockHitResult = Layers.HitTest(Layers.BlockLayer.Shapes.Cast<XBlock>(), p);
                }
            }

            if (shapeHitResult != null
                && pinHitResult == null
                && wireHitResult == null
                && blockHitResult == null)
            {
                if (shapeHitResult is XBlock)
                {
                    XBlock block = shapeHitResult as XBlock;

                    Layers.BlockLayer.Hidden.Add(shapeHitResult);
                    Layers.BlockLayer.InvalidateVisual();

                    Layers.OverlayLayer.Shapes.Add(block);
                    Layers.OverlayLayer.InvalidateVisual();
                }
                else if (shapeHitResult is XPin)
                {
                    XPin pin = shapeHitResult as XPin;
                    if (Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire)
                    {
                        Layers.BlockLayer.Hidden.Add(pin);
                        Layers.PinLayer.Hidden.Add(pin);
                        Layers.BlockLayer.InvalidateVisual();
                        Layers.PinLayer.InvalidateVisual();

                        Layers.OverlayLayer.Shapes.Add(pin);
                        Layers.OverlayLayer.InvalidateVisual();
                    }
                }
                else if (shapeHitResult is XWire)
                {
                    if (Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire || Layers.Tool.CurrentTool == ToolMenuModel.Tool.Pin)
                    {
                        Layers.WireLayer.Hidden.Add(wireHitResult);
                        Layers.WireLayer.InvalidateVisual();

                        Layers.OverlayLayer.Shapes.Add(wireHitResult);
                        Layers.OverlayLayer.InvalidateVisual();
                    }
                }
                else
                {
                    if (Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire)
                    {
                        Layers.ShapeLayer.Hidden.Add(shapeHitResult);
                        Layers.ShapeLayer.InvalidateVisual();

                        Layers.OverlayLayer.Shapes.Add(shapeHitResult);
                        Layers.OverlayLayer.InvalidateVisual();
                    }
                }
            }

            if (pinHitResult != null)
            {
                XPin pin = pinHitResult as XPin;
                if (Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire)
                {
                    Layers.PinLayer.Hidden.Add(pin);
                    Layers.BlockLayer.Hidden.Add(pin);
                    Layers.PinLayer.InvalidateVisual();
                    Layers.BlockLayer.InvalidateVisual();

                    Layers.OverlayLayer.Shapes.Add(pin);
                    Layers.OverlayLayer.InvalidateVisual();
                }
            }
            else if (wireHitResult != null)
            {
                if (wireHitResult is XWire)
                {
                    if (Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire || Layers.Tool.CurrentTool == ToolMenuModel.Tool.Pin)
                    {
                        Layers.WireLayer.Hidden.Add(wireHitResult);
                        Layers.WireLayer.InvalidateVisual();

                        Layers.OverlayLayer.Shapes.Add(wireHitResult);
                        Layers.OverlayLayer.InvalidateVisual();
                    }
                }
                else if (wireHitResult is XPin)
                {
                    XPin pin = wireHitResult as XPin;
                    if (Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire)
                    {
                        if (pin.Owner == null)
                        {
                            Layers.PinLayer.Hidden.Add(pin);
                            Layers.PinLayer.InvalidateVisual();
                        }
                        else
                        {
                            Layers.BlockLayer.Hidden.Add(pin);
                            Layers.BlockLayer.InvalidateVisual();
                        }

                        Layers.OverlayLayer.Shapes.Add(pin);
                        Layers.OverlayLayer.InvalidateVisual();
                    }
                }
            }
            else if (blockHitResult != null)
            {
                if (blockHitResult is XBlock)
                {
                    XBlock block = shapeHitResult as XBlock;
                    if (Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire)
                    {
                        Layers.BlockLayer.Hidden.Add(block);
                        Layers.BlockLayer.InvalidateVisual();

                        Layers.OverlayLayer.Shapes.Add(block);
                        Layers.OverlayLayer.InvalidateVisual();
                    }
                }
                else if (blockHitResult is XPin)
                {
                    XPin pin = blockHitResult as XPin;
                    if (Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire)
                    {
                        Layers.BlockLayer.Hidden.Add(pin);
                        Layers.BlockLayer.InvalidateVisual();

                        Layers.OverlayLayer.Shapes.Add(pin);
                        Layers.OverlayLayer.InvalidateVisual();
                    }
                }
            }
        }

        private void OverlayReset()
        {
            if (Layers.OverlayLayer.Shapes.Count > 0)
            {
                if (Layers.ShapeLayer.Hidden != null && Layers.ShapeLayer.Hidden.Count > 0)
                {
                    Layers.ShapeLayer.Hidden.Clear();
                    Layers.ShapeLayer.InvalidateVisual();
                }

                if (Layers.BlockLayer.Hidden != null && Layers.BlockLayer.Hidden.Count > 0)
                {
                    Layers.BlockLayer.Hidden.Clear();
                    Layers.BlockLayer.InvalidateVisual();
                }

                if (Layers.WireLayer.Hidden != null && Layers.WireLayer.Hidden.Count > 0)
                {
                    Layers.WireLayer.Hidden.Clear();
                    Layers.WireLayer.InvalidateVisual();
                }

                if (Layers.PinLayer.Hidden != null && Layers.PinLayer.Hidden.Count > 0)
                {
                    Layers.PinLayer.Hidden.Clear();
                    Layers.PinLayer.InvalidateVisual();
                }

                Layers.PinLayer.Hidden.Clear();
                Layers.OverlayLayer.Shapes.Clear();
                Layers.OverlayLayer.InvalidateVisual();
            }
        }

        #endregion

        #region Create Mode

        private void CreateWireInit(Point2 p)
        {
            IShape pinHitResult = null;
            IShape wireHitResult = null;
            IShape blockHitResult = null;

            if (Layers != null)
            {
                pinHitResult = Layers.HitTest(Layers.PinLayer.Shapes.Cast<XPin>(), p);
                if (pinHitResult == null)
                {
                    wireHitResult = Layers.HitTest(Layers.WireLayer.Shapes.Cast<XWire>(), p);
                    if (wireHitResult == null)
                    {
                        blockHitResult = Layers.HitTest(Layers.BlockLayer.Shapes.Cast<XBlock>(), p);
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
                if (Layers.PinLayer != null)
                {
                    // create new standalone pin
                    pinHitResult = new XPin()
                    {
                        Name = "P",
                        PinType = PinType.Standalone,
                        Owner = null,
                        X = x,
                        Y = y
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
                    if (Layers.PinLayer != null && Layers.WireLayer != null)
                    {
                        WireSplitStart(wireHitResult, x, y);
                    }
                }
                else if (blockHitResult != null && blockHitResult is XPin)
                {
                    _wire.Start = blockHitResult as XPin;
                }
            }
        }

        private void CreateWireFinish(Point2 p)
        {
            IShape pinHitResult = null;
            IShape wireHitResult = null;
            IShape blockHitResult = null;

            if (Layers != null)
            {
                pinHitResult = Layers.HitTest(Layers.PinLayer.Shapes.Cast<XPin>(), p);
                if (pinHitResult == null)
                {
                    wireHitResult = Layers.HitTest(Layers.WireLayer.Shapes.Cast<XWire>(), p);
                    if (wireHitResult == null)
                    {
                        blockHitResult = Layers.HitTest(Layers.BlockLayer.Shapes.Cast<XBlock>(), p);
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
                if (Layers.PinLayer != null)
                {
                    // create new standalone pin
                    pinHitResult = new XPin()
                    {
                        Name = "P",
                        PinType = PinType.Standalone,
                        Owner = null,
                        X = x,
                        Y = y
                    };

                    Layers.PinLayer.Shapes.Add(pinHitResult);
                    Layers.PinLayer.InvalidateVisual();
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
                if (Layers.PinLayer != null && Layers.WireLayer != null)
                {
                    WireSplitEnd(wireHitResult, x, y);
                }
            }
            else if (blockHitResult != null && blockHitResult is XPin)
            {
                _wire.End = blockHitResult as XPin;
            }
        }

        private void CreateInit(Point2 p)
        {
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            switch (Layers.Tool.CurrentTool)
            {
                case ToolMenuModel.Tool.Line:
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
                case ToolMenuModel.Tool.Ellipse:
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
                case ToolMenuModel.Tool.Rectangle:
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
                case ToolMenuModel.Tool.Text:
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
                case ToolMenuModel.Tool.Image:
                    {
                        string path = GetFilePath();
                        if (path == null)
                            return;

                        _startx = x;
                        _starty = y;
                        _image = new XImage()
                        {
                            X = x,
                            Y = y,
                            Width = 0.0,
                            Height = 0.0,
                            Path = new Uri(path)
                        };
                        Shapes.Add(_image);
                        CaptureMouse();
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Wire:
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
                case ToolMenuModel.Tool.Pin:
                    {
                        // create new standalone pin
                        _pin = new XPin()
                        {
                            Name = "P",
                            PinType = PinType.Standalone,
                            Owner = null,
                            X = x,
                            Y = y
                        };
                        Shapes.Add(_pin);
                        CaptureMouse();
                        InvalidateVisual();
                    }
                    break;
            }
            _mode = Mode.Create;
        }

        private void CreateMove(Point2 p)
        {
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            switch (Layers.Tool.CurrentTool)
            {
                case ToolMenuModel.Tool.Line:
                    {
                        _line.X2 = x;
                        _line.Y2 = y;
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Ellipse:
                    {
                        _ellipse.RadiusX = Math.Abs(x - _startx) / 2.0;
                        _ellipse.RadiusY = Math.Abs(y - _starty) / 2.0;
                        _ellipse.X = Math.Min(_startx, x) + _ellipse.RadiusX;
                        _ellipse.Y = Math.Min(_starty, y) + _ellipse.RadiusY;
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Rectangle:
                    {
                        _rectangle.X = Math.Min(_startx, x);
                        _rectangle.Y = Math.Min(_starty, y);
                        _rectangle.Width = Math.Abs(x - _startx);
                        _rectangle.Height = Math.Abs(y - _starty);
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Text:
                    {
                        _text.X = Math.Min(_startx, x);
                        _text.Y = Math.Min(_starty, y);
                        _text.Width = Math.Abs(x - _startx);
                        _text.Height = Math.Abs(y - _starty);
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Image:
                    {
                        _image.X = Math.Min(_startx, x);
                        _image.Y = Math.Min(_starty, y);
                        _image.Width = Math.Abs(x - _startx);
                        _image.Height = Math.Abs(y - _starty);
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Wire:
                    {
                        _wire.X2 = x;
                        _wire.Y2 = y;
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Pin:
                    {
                        _pin.X = x;
                        _pin.Y = y;
                        InvalidateVisual();
                    }
                    break;
            }
        }

        private void CreateFinish(Point2 p)
        {
            double x = EnableSnap ? Snap(p.X, SnapSize) : p.X;
            double y = EnableSnap ? Snap(p.Y, SnapSize) : p.Y;

            switch (Layers.Tool.CurrentTool)
            {
                case ToolMenuModel.Tool.Line:
                    {
                        _line.X2 = x;
                        _line.Y2 = y;
                        if (Layers.ShapeLayer != null)
                        {
                            Shapes.Remove(_line);
                            Layers.Snapshot();
                            Layers.ShapeLayer.Shapes.Add(_line);
                            Layers.ShapeLayer.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Ellipse:
                    {
                        _ellipse.RadiusX = Math.Abs(x - _startx) / 2.0;
                        _ellipse.RadiusY = Math.Abs(y - _starty) / 2.0;
                        _ellipse.X = Math.Min(_startx, x) + _ellipse.RadiusX;
                        _ellipse.Y = Math.Min(_starty, y) + _ellipse.RadiusY;
                        if (Layers.ShapeLayer != null)
                        {
                            Shapes.Remove(_ellipse);
                            Layers.Snapshot();
                            Layers.ShapeLayer.Shapes.Add(_ellipse);
                            Layers.ShapeLayer.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Rectangle:
                    {
                        _rectangle.X = Math.Min(_startx, x);
                        _rectangle.Y = Math.Min(_starty, y);
                        _rectangle.Width = Math.Abs(x - _startx);
                        _rectangle.Height = Math.Abs(y - _starty);
                        if (Layers.ShapeLayer != null)
                        {
                            Shapes.Remove(_rectangle);
                            Layers.Snapshot();
                            Layers.ShapeLayer.Shapes.Add(_rectangle);
                            Layers.ShapeLayer.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Text:
                    {
                        _text.X = Math.Min(_startx, x);
                        _text.Y = Math.Min(_starty, y);
                        _text.Width = Math.Abs(x - _startx);
                        _text.Height = Math.Abs(y - _starty);
                        if (Layers.ShapeLayer != null)
                        {
                            Shapes.Remove(_text);
                            Layers.Snapshot();
                            Layers.ShapeLayer.Shapes.Add(_text);
                            Layers.ShapeLayer.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Image:
                    {
                        _image.X = Math.Min(_startx, x);
                        _image.Y = Math.Min(_starty, y);
                        _image.Width = Math.Abs(x - _startx);
                        _image.Height = Math.Abs(y - _starty);
                        if (Layers.ShapeLayer != null)
                        {
                            Shapes.Remove(_image);
                            Layers.Snapshot();
                            Layers.ShapeLayer.Shapes.Add(_image);
                            Layers.ShapeLayer.InvalidateVisual();
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Wire:
                    {
                        _wire.X2 = x;
                        _wire.Y2 = y;
                        if (Layers.WireLayer != null)
                        {
                            Shapes.Remove(_wire);

                            if (_pin != null)
                            {
                                Shapes.Remove(_pin);
                            }

                            Layers.Snapshot();
                            Layers.WireLayer.Shapes.Add(_wire);
                            Layers.WireLayer.InvalidateVisual();

                            if (_pin != null)
                            {
                                Layers.PinLayer.Shapes.Add(_pin);
                                Layers.PinLayer.InvalidateVisual();
                            }
                        }
                        ReleaseMouseCapture();
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Pin:
                    {
                        _pin.X = x;
                        _pin.Y = y;
                        if (Layers.PinLayer != null)
                        {
                            Shapes.Remove(_pin);
                            Layers.Snapshot();
                            Layers.PinLayer.Shapes.Add(_pin);
                            Layers.PinLayer.InvalidateVisual();
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
            switch (Layers.Tool.CurrentTool)
            {
                case ToolMenuModel.Tool.Line:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_line);
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Ellipse:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_ellipse);
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Rectangle:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_rectangle);
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Text:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_text);
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Image:
                    {
                        ReleaseMouseCapture();
                        Shapes.Remove(_image);
                        InvalidateVisual();
                    }
                    break;
                case ToolMenuModel.Tool.Wire:
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
                case ToolMenuModel.Tool.Pin:
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

        #region Shape Properties

        public void ShapeToggleFill()
        {
            if (IsMouseCaptured() && Layers.Tool.CurrentTool == ToolMenuModel.Tool.Rectangle)
            {
                _rectangle.IsFilled = !_rectangle.IsFilled;
                InvalidateVisual();
            }
            else if (IsMouseCaptured() && Layers.Tool.CurrentTool == ToolMenuModel.Tool.Ellipse)
            {
                _ellipse.IsFilled = !_ellipse.IsFilled;
                InvalidateVisual();
            }
            else if (IsMouseCaptured() && Layers.Tool.CurrentTool == ToolMenuModel.Tool.Text)
            {
                _text.IsFilled = !_text.IsFilled;
                InvalidateVisual();
            }
            else
            {
                ShapeToggleSelectedFill();
            }
        }

        public void ShapeToggleInvertStart()
        {
            if (IsMouseCaptured() && Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire)
            {
                _wire.InvertStart = !_wire.InvertStart;
                InvalidateVisual();
            }
            else
            {
                ShapeToggleSelectedInvertStart();
            }
        }

        public void ShapeToggleInvertEnd()
        {
            if (IsMouseCaptured() && Layers.Tool.CurrentTool == ToolMenuModel.Tool.Wire)
            {
                _wire.InvertEnd = !_wire.InvertEnd;
                InvalidateVisual();
            }
            else
            {
                ShapeToggleSelectedInvertEnd();
            }
        }

        public void ShapeSetTextSizeDelta(double delta)
        {
            if (IsMouseCaptured() && Layers.Tool.CurrentTool == ToolMenuModel.Tool.Text)
            {
                double size = _text.FontSize + delta;
                if (size > 0.0)
                {
                    _text.FontSize = size;
                    InvalidateVisual();
                }
            }
            else
            {
                ShapeSetSelectedTextSizeDelta(delta);
            }
        }

        public void ShapeSetTextHAlignment(HAlignment halignment)
        {
            if (IsMouseCaptured() && Layers.Tool.CurrentTool == ToolMenuModel.Tool.Text)
            {
                _text.HAlignment = halignment;
                InvalidateVisual();
            }
            else
            {
                ShapeSetSelectedTextHAlignment(halignment);
            }
        }

        public void ShapeSetTextVAlignment(VAlignment valignment)
        {
            if (IsMouseCaptured() && Layers.Tool.CurrentTool == ToolMenuModel.Tool.Text)
            {
                _text.VAlignment = valignment;
                InvalidateVisual();
            }
            else
            {
                ShapeSetSelectedTextVAlignment(valignment);
            }
        }

        public void ShapeToggleSelectedFill()
        {
            if (Layers.HaveSelection())
            {
                var rectangles = Renderer.Selected.Where(x => x is XRectangle).Cast<XRectangle>();
                foreach (var rectangle in rectangles)
                {
                    rectangle.IsFilled = !rectangle.IsFilled;
                }

                var ellipses = Renderer.Selected.Where(x => x is XEllipse).Cast<XEllipse>();
                foreach (var ellipse in ellipses)
                {
                    ellipse.IsFilled = !ellipse.IsFilled;
                }

                var texts = Renderer.Selected.Where(x => x is XText).Cast<XText>();
                foreach (var text in texts)
                {
                    text.IsFilled = !text.IsFilled;
                }

                Layers.ShapeLayer.InvalidateVisual();
            }
        }

        public void ShapeToggleSelectedInvertStart()
        {
            if (Layers.HaveSelection())
            {
                var wires = Renderer.Selected.Where(x => x is XWire).Cast<XWire>();
                foreach (var wire in wires)
                {
                    wire.InvertStart = !wire.InvertStart;
                }
                Layers.WireLayer.InvalidateVisual();
            }
        }

        public void ShapeToggleSelectedInvertEnd()
        {
            if (Layers.HaveSelection())
            {
                var wires = Renderer.Selected.Where(x => x is XWire).Cast<XWire>();
                foreach (var wire in wires)
                {
                    wire.InvertEnd = !wire.InvertEnd;
                }
                Layers.WireLayer.InvalidateVisual();
            }
        }

        public void ShapeSetSelectedTextSizeDelta(double delta)
        {
            if (Layers.HaveSelection())
            {
                var texts = Renderer.Selected.Where(x => x is XText).Cast<XText>();
                foreach (var text in texts)
                {
                    double size = text.FontSize + delta;
                    if (size > 0.0)
                    {
                        text.FontSize = size;
                    }
                }
                Layers.ShapeLayer.InvalidateVisual();
            }
        }

        public void ShapeSetSelectedTextHAlignment(HAlignment halignment)
        {
            if (Layers.HaveSelection())
            {
                var texts = Renderer.Selected.Where(x => x is XText).Cast<XText>();
                foreach (var text in texts)
                {
                    text.HAlignment = halignment;
                }
                Layers.ShapeLayer.InvalidateVisual();
            }
        }

        public void ShapeSetSelectedTextVAlignment(VAlignment valignment)
        {
            if (Layers.HaveSelection())
            {
                var texts = Renderer.Selected.Where(x => x is XText).Cast<XText>();
                foreach (var text in texts)
                {
                    text.VAlignment = valignment;
                }
                Layers.ShapeLayer.InvalidateVisual();
            }
        }

        #endregion

        #region Snap

        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        #endregion

        #region Min

        public void Min(ICollection<IShape> shapes, ref double x, ref double y)
        {
            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    var line = shape as XLine;
                    x = Math.Min(x, line.X1);
                    y = Math.Min(y, line.Y1);
                    x = Math.Min(x, line.X2);
                    y = Math.Min(y, line.Y2);
                }
                else if (shape is XEllipse)
                {
                    var ellipse = shape as XEllipse;
                    x = Math.Min(x, ellipse.X);
                    y = Math.Min(y, ellipse.Y);
                }
                else if (shape is XRectangle)
                {
                    var rectangle = shape as XRectangle;
                    x = Math.Min(x, rectangle.X);
                    y = Math.Min(y, rectangle.Y);
                }
                else if (shape is XText)
                {
                    var text = shape as XText;
                    x = Math.Min(x, text.X);
                    y = Math.Min(y, text.Y);
                }
                else if (shape is XImage)
                {
                    var image = shape as XImage;
                    x = Math.Min(x, image.X);
                    y = Math.Min(y, image.Y);
                }
                else if (shape is XWire)
                {
                    var wire = shape as XWire;
                    if (wire.Start == null)
                    {
                        x = Math.Min(x, wire.X1);
                        y = Math.Min(y, wire.Y1);
                    }
                    if (wire.End == null)
                    {
                        x = Math.Min(x, wire.X2);
                        y = Math.Min(y, wire.Y2);
                    }
                }
                else if (shape is XPin)
                {
                    var pin = shape as XPin;
                    x = Math.Min(x, pin.X);
                    y = Math.Min(y, pin.Y);
                }
                else if (shape is XBlock)
                {
                    var block = shape as XBlock;
                    Min(block.Shapes, ref x, ref y);
                    foreach (var pin in block.Pins)
                    {
                        x = Math.Min(x, pin.X);
                        y = Math.Min(y, pin.Y);
                    }
                }
            }
        }

        #endregion

        #region Wire

        private void WireSplit(XWire wire, double x, double y, out XPin pin, out XWire split)
        {
            // create new standalone pin
            pin = new XPin()
            {
                Name = "P",
                PinType = PinType.Standalone,
                Owner = null,
                X = x,
                Y = y
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

        private void WireSplitStart(IShape wireHitResult, double x, double y)
        {
            XPin pin;
            XWire split;

            WireSplit(wireHitResult as XWire, x, y, out pin, out split);

            _wire.Start = pin;

            Layers.PinLayer.Shapes.Add(pin);
            Layers.WireLayer.Shapes.Add(split);

            Layers.WireLayer.InvalidateVisual();
            Layers.PinLayer.InvalidateVisual();
        }

        private void WireSplitEnd(IShape wireHitResult, double x, double y)
        {
            XPin pin;
            XWire split;

            WireSplit(wireHitResult as XWire, x, y, out pin, out split);

            _wire.End = pin;

            Layers.PinLayer.Shapes.Add(pin);
            Layers.WireLayer.Shapes.Add(split);

            Layers.WireLayer.InvalidateVisual();
            Layers.PinLayer.InvalidateVisual();
        }

        #endregion

        #region Block

        public XBlock Insert(XBlock block, double x, double y)
        {
            // clone block
            XBlock copy = Layers.Clone(block);

            if (copy != null)
            {
                // move to drop position
                double dx = EnableSnap ? Snap(x, SnapSize) : x;
                double dy = EnableSnap ? Snap(y, SnapSize) : y;
                Move(copy, dx, dy);

                // add to collection
                Layers.BlockLayer.Shapes.Add(copy);
                Layers.BlockLayer.InvalidateVisual();

                return copy;
            }

            return null;
        }

        private void Split(XWire wire, XPin pin0, XPin pin1)
        {
            // pins must be aligned horizontally or vertically
            if (pin0.X != pin1.X && pin0.Y != pin1.Y)
                return;

            // wire must be horizontal or vertical
            if (wire.Start.X != wire.End.X && wire.Start.Y != wire.End.Y)
                return;

            XWire split;
            if (wire.Start.X > wire.End.X || wire.Start.Y > wire.End.Y)
            {
                split = new XWire()
                {
                    Start = pin0,
                    End = wire.End,
                    InvertStart = false,
                    InvertEnd = wire.InvertEnd
                };
                wire.InvertEnd = false;
                wire.End = pin1;
            }
            else
            {
                split = new XWire()
                {
                    Start = pin1,
                    End = wire.End,
                    InvertStart = false,
                    InvertEnd = wire.InvertEnd
                };
                wire.InvertEnd = false;
                wire.End = pin0;
            }
            Layers.WireLayer.Shapes.Add(split);
            Layers.PinLayer.InvalidateVisual();
            Layers.WireLayer.InvalidateVisual();
        }

        public void Connect(XBlock block)
        {
            // check for pin to wire connections
            int count = block.Pins.Count();
            if (count > 0)
            {
                var wires = Layers.WireLayer.Shapes.Cast<XWire>();
                var dict = new Dictionary<XWire, List<XPin>>();

                // find connections
                foreach (var pin in block.Pins)
                {
                    IShape hit = Layers.HitTest(wires, new Point2(pin.X, pin.Y));
                    if (hit != null && hit is XWire)
                    {
                        var wire = hit as XWire;
                        if (dict.ContainsKey(wire))
                        {
                            dict[wire].Add(pin);
                        }
                        else
                        {
                            dict.Add(wire, new List<XPin>());
                            dict[wire].Add(pin);
                        }
                    }
                }

                // split wires
                foreach (var kv in dict)
                {
                    List<XPin> pins = kv.Value;
                    if (pins.Count == 2)
                    {
                        Split(kv.Key, pins[0], pins[1]);
                    }
                }
            }
        }

        #endregion

        #region Block

        public XBlock BlockCreateFromSelected(string name)
        {
            if (Layers.HaveSelection())
            {
                return BlockCreate(name, Renderer.Selected);
            }
            return null;
        }

        public XBlock BlockCreate(string name, IEnumerable<IShape> shapes)
        {
            var block = new XBlock()
            {
                Name = name,
                Shapes = new ObservableCollection<IShape>(),
                Pins = new ObservableCollection<XPin>()
            };

            foreach (var shape in shapes)
            {
                if (shape is XLine)
                {
                    block.Shapes.Add(shape);
                }
                else if (shape is XEllipse)
                {
                    block.Shapes.Add(shape);
                }
                else if (shape is XRectangle)
                {
                    block.Shapes.Add(shape);
                }
                else if (shape is XText)
                {
                    block.Shapes.Add(shape);
                }
                else if (shape is XImage)
                {
                    block.Shapes.Add(shape);
                }
                else if (shape is XWire)
                {
                    block.Shapes.Add(shape);
                }
                else if (shape is XPin)
                {
                    block.Pins.Add(shape as XPin);
                }
                else if (shape is XBlock)
                {
                    // Not supported.
                }
            }
            return block;
        }

        private void BlockToggleState(Point2 p)
        {
            IShape shape = Layers != null ? Layers.HitTest(p) : null;
            if (shape is XBlock)
            {
                var block = shape as XBlock;
                var simulation = Simulations[block];

                bool? state = simulation.State;
                simulation.State = !state;

                if (Layers.IsSimulationPaused)
                {
                    Layers.OverlayLayer.InvalidateVisual();
                }
            }
        }

        #endregion

        #region Render

        private void RenderSimulationMode(object dc)
        {
            foreach (var shape in Shapes)
            {
                if (shape is XBlock)
                {
                    var block = shape as XBlock;
                    bool? state = Simulations[block].State;
                    IStyle style;
                    switch (state)
                    {
                        case true:
                            style = TrueStateStyle;
                            break;
                        case false:
                            style = FalseStateStyle;
                            break;
                        case null:
                        default:
                            style = NullStateStyle;
                            break;
                    }
                    block.Render(dc, Renderer, style);
                    foreach (var pin in block.Pins)
                    {
                        pin.Render(dc, Renderer, style);
                    }
                }
            }
        }

        private void RenderNormalMode(object dc, IStyle style)
        {
            foreach (var shape in Shapes)
            {
                shape.Render(dc, Renderer, style);
                if (shape is XBlock)
                {
                    foreach (var pin in (shape as XBlock).Pins)
                    {
                        pin.Render(dc, Renderer, style);
                    }
                }
            }
        }

        private void RenderSelectedMode(object dc, IStyle normal, IStyle selected)
        {
            foreach (var shape in Shapes)
            {
                IStyle style = Renderer.Selected.Contains(shape) ? selected : normal;
                shape.Render(dc, Renderer, style);
                if (shape is XBlock)
                {
                    foreach (var pin in (shape as XBlock).Pins)
                    {
                        pin.Render(dc, Renderer, style);
                    }
                }
            }
        }

        private void RenderHiddenMode(object dc, IStyle style)
        {
            foreach (var shape in Shapes)
            {
                if (!Hidden.Contains(shape))
                {
                    shape.Render(dc, Renderer, style);
                    if (shape is XBlock)
                    {
                        foreach (var pin in (shape as XBlock).Pins)
                        {
                            if (!Hidden.Contains(pin))
                            {
                                pin.Render(dc, Renderer, style);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
