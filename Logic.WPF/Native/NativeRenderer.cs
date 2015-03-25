// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Logic.Native
{
    public class NativeRenderer : IRenderer
    {
        #region IRenderer

        public IList<KeyValuePair<string, IProperty>> Database { get; set; }
        public ICollection<IShape> Selected { get; set; }
        public double Zoom { get; set; }
        public double InvertSize { get; set; }
        public double PinRadius { get; set; }
        public double HitTreshold { get; set; }
        public bool ShortenWire { get; set; }
        public double ShortenSize { get; set; }

        public void DrawLine(object dc, IStyle style, XLine line)
        {
            double thickness = style.Thickness / Zoom;
            double half = thickness / 2.0;

            var pen = new Pen(
                new SolidColorBrush(
                    Color.FromArgb(
                        (byte)style.Stroke.A,
                        (byte)style.Stroke.R,
                        (byte)style.Stroke.G,
                        (byte)style.Stroke.B)),
                    thickness);
            pen.Freeze();

            var gs = new GuidelineSet(
                new double[] { line.X1 + half, line.X2 + half },
                new double[] { line.Y1 + half, line.Y2 + half });
            (dc as DrawingContext).PushGuidelineSet(gs);

            (dc as DrawingContext).DrawLine(
                pen,
                new Point(line.X1, line.Y1),
                new Point(line.X2, line.Y2));

            (dc as DrawingContext).Pop();
        }

        public void DrawEllipse(object dc, IStyle style, XEllipse ellipse)
        {
            double thickness = style.Thickness / Zoom;

            var pen = new Pen(
                new SolidColorBrush(
                    Color.FromArgb(
                        (byte)style.Stroke.A,
                        (byte)style.Stroke.R,
                        (byte)style.Stroke.G,
                        (byte)style.Stroke.B)),
                    thickness);
            pen.Freeze();

            if (ellipse.IsFilled)
            {
                var brush = new SolidColorBrush(
                    Color.FromArgb(
                        (byte)style.Fill.A,
                        (byte)style.Fill.R,
                        (byte)style.Fill.G,
                        (byte)style.Fill.B));
                brush.Freeze();

                (dc as DrawingContext).DrawEllipse(
                    brush,
                    pen,
                    new Point(ellipse.X, ellipse.Y),
                    ellipse.RadiusX,
                    ellipse.RadiusY);
            }
            else
            {
                (dc as DrawingContext).DrawEllipse(
                    null,
                    pen,
                    new Point(ellipse.X, ellipse.Y),
                    ellipse.RadiusX,
                    ellipse.RadiusY);
            }
        }

        public void DrawRectangle(object dc, IStyle style, XRectangle rectangle)
        {
            double thickness = style.Thickness / Zoom;
            double half = thickness / 2.0;

            var pen = new Pen(
                new SolidColorBrush(
                    Color.FromArgb(
                        (byte)style.Stroke.A,
                        (byte)style.Stroke.R,
                        (byte)style.Stroke.G,
                        (byte)style.Stroke.B)),
                    thickness);
            pen.Freeze();

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

            if (rectangle.IsFilled)
            {
                var brush = new SolidColorBrush(
                    Color.FromArgb(
                        (byte)style.Fill.A,
                        (byte)style.Fill.R,
                        (byte)style.Fill.G,
                        (byte)style.Fill.B));
                brush.Freeze();

                (dc as DrawingContext).DrawRectangle(
                    brush,
                    pen,
                    new Rect(
                        rectangle.X,
                        rectangle.Y,
                        rectangle.Width,
                        rectangle.Height));
            }
            else
            {
                (dc as DrawingContext).DrawRectangle(
                    null,
                    pen,
                    new Rect(
                        rectangle.X,
                        rectangle.Y,
                        rectangle.Width,
                        rectangle.Height));
            }

            (dc as DrawingContext).Pop();
        }

        public void DrawText(object dc, IStyle style, XText text)
        {
            var foreground = new SolidColorBrush(
                Color.FromArgb(
                    (byte)style.Stroke.A,
                    (byte)style.Stroke.R,
                    (byte)style.Stroke.G,
                    (byte)style.Stroke.B));
            foreground.Freeze();

            var ft = new FormattedText(
                text.Bind(Database),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(text.FontName),
                text.FontSize,
                foreground,
                null,
                TextFormattingMode.Ideal);

            double x = text.X;
            double y = text.Y;

            switch (text.HAlignment)
            {
                case HAlignment.Left:
                    break;
                case HAlignment.Center:
                    x += text.Width / 2.0 - ft.Width / 2.0;
                    break;
                case HAlignment.Right:
                    x += text.Width - ft.Width;
                    break;
            }

            switch (text.VAlignment)
            {
                case VAlignment.Top:
                    break;
                case VAlignment.Center:
                    y += text.Height / 2.0 - ft.Height / 2.0;
                    break;
                case VAlignment.Bottom:
                    y += text.Height - ft.Height;
                    break;
            }

            if (text.IsFilled)
            {
                var background = new SolidColorBrush(
                    Color.FromArgb(
                        (byte)style.Fill.A,
                        (byte)style.Fill.R,
                        (byte)style.Fill.G,
                        (byte)style.Fill.B));
                background.Freeze();

                (dc as DrawingContext).DrawRectangle(
                    background,
                    null,
                    new Rect(
                        text.X,
                        text.Y,
                        text.Width,
                        text.Height));
            }

            (dc as DrawingContext).DrawText(
                ft,
                new Point(x, y));
        }

        public void DrawImage(object dc, IStyle style, XImage image)
        {
            if (image.Path == null)
                return;

            if (!_biCache.ContainsKey(image.Path))
            {
                byte[] buffer = System.IO.File.ReadAllBytes(image.Path.LocalPath);
                var ms = new System.IO.MemoryStream(buffer);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                _biCache[image.Path] = bi;
            }

            (dc as DrawingContext).DrawImage(
                _biCache[image.Path],
                new Rect(
                    image.X,
                    image.Y,
                    image.Width,
                    image.Height));
        }

        public void DrawPin(object dc, IStyle style, XPin pin)
        {
            double thickness = style.Thickness / Zoom;
            double half = thickness / 2.0;

            var pen = new Pen(
                new SolidColorBrush(
                    Color.FromArgb(
                        (byte)style.Stroke.A,
                        (byte)style.Stroke.R,
                        (byte)style.Stroke.G,
                        (byte)style.Stroke.B)),
                    thickness);
            pen.Freeze();

            var brush = new SolidColorBrush(
                Color.FromArgb(
                    (byte)style.Fill.A,
                    (byte)style.Fill.R,
                    (byte)style.Fill.G,
                    (byte)style.Fill.B));
            brush.Freeze();

            (dc as DrawingContext).DrawEllipse(
                brush,
                pen,
                new Point(pin.X, pin.Y),
                PinRadius,
                PinRadius);
        }

        public void DrawWire(object dc, IStyle style, XWire wire)
        {
            double thickness = style.Thickness / Zoom;
            double half = thickness / 2.0;

            var pen = new Pen(
                new SolidColorBrush(
                    Color.FromArgb(
                        (byte)style.Stroke.A,
                        (byte)style.Stroke.R,
                        (byte)style.Stroke.G,
                        (byte)style.Stroke.B)),
                    thickness);
            pen.Freeze();

            var position = WirePosition.Calculate(
                wire, 
                InvertSize,
                ShortenWire,
                ShortenSize);

            if (wire.InvertStart)
            {
                (dc as DrawingContext).DrawEllipse(
                    null,
                    pen,
                    new Point(
                        position.InvertX1,
                        position.InvertY1),
                    InvertSize,
                    InvertSize);
            }

            if (wire.InvertEnd)
            {
                (dc as DrawingContext).DrawEllipse(
                    null,
                    pen,
                    new Point(
                        position.InvertX2,
                        position.InvertY2),
                    InvertSize,
                    InvertSize);
            }

            var gs = new GuidelineSet(
                new double[] { position.StartX + half, position.StartY + half },
                new double[] { position.EndX + half, position.EndY + half });
            (dc as DrawingContext).PushGuidelineSet(gs);

            (dc as DrawingContext).DrawLine(
                pen,
                new Point(
                    position.StartX,
                    position.StartY),
                new Point(
                    position.EndX,
                    position.EndY));

            (dc as DrawingContext).Pop();
        } 

        #endregion

        #region IDisposable

        private IDictionary<Uri, BitmapImage> _biCache = new Dictionary<Uri, BitmapImage>();

        public void Dispose()
        {
            foreach (var kvp in _biCache)
            {
                kvp.Value.StreamSource.Dispose();
            }
            _biCache.Clear();
        }

        #endregion
    }
}
