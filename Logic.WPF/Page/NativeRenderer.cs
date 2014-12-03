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

namespace Logic.Page
{
    public class NativeRenderer : IRenderer
    {
        #region IRenderer

        public IList<KeyValuePair<string, IProperty>> Database { get; set; }
        public ICollection<IShape> Selected { get; set; }
        public double InvertSize { get; set; }
        public double PinRadius { get; set; }
        public double HitTreshold { get; set; }

        public void DrawLine(object dc, IStyle style, XLine line)
        {
            double thickness = style.Thickness;
            double half = thickness / 2.0;
            var gs = new GuidelineSet(
                new double[] { half, half },
                new double[] { half, half });
            (dc as DrawingContext).PushGuidelineSet(gs);

            (dc as DrawingContext).DrawLine(
                (Pen)style.NativeStroke(),
                new Point(line.X1, line.Y1),
                new Point(line.X2, line.Y2));

            (dc as DrawingContext).Pop();
        }

        public void DrawEllipse(object dc, IStyle style, XEllipse ellipse)
        {
            (dc as DrawingContext).DrawEllipse(
                ellipse.IsFilled ? (SolidColorBrush)style.NativeFill() : null,
                (Pen)style.NativeStroke(),
                new Point(ellipse.X, ellipse.Y),
                ellipse.RadiusX,
                ellipse.RadiusY);
        }

        public void DrawRectangle(object dc, IStyle style, XRectangle rectangle)
        {
            double thickness = style.Thickness;
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
                rectangle.IsFilled ? (SolidColorBrush)style.NativeFill() : null,
                (Pen)style.NativeStroke(),
                new Rect(
                    rectangle.X,
                    rectangle.Y,
                    rectangle.Width,
                    rectangle.Height));

            (dc as DrawingContext).Pop();
        }

        public void DrawText(object dc, IStyle style, XText text)
        {
            var ft = new FormattedText(
                text.Bind(Database),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(text.FontName),
                text.FontSize,
                ((Pen)style.NativeStroke()).Brush,
                null,
                TextFormattingMode.Ideal);

            double x = text.X;
            double y = text.Y;

            // horizontal alignment
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

            // vertical alignment
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
                (dc as DrawingContext).DrawRectangle(
                    (SolidColorBrush)style.NativeFill(),
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
            (dc as DrawingContext).DrawEllipse(
                (SolidColorBrush)style.NativeFill(),
                (Pen)style.NativeStroke(),
                new Point(pin.X, pin.Y),
                PinRadius,
                PinRadius);
        }

        public void DrawWire(object dc, IStyle style, XWire wire)
        {
            var position = WirePosition.Calculate(wire, InvertSize);

            if (wire.InvertStart)
            {
                (dc as DrawingContext).DrawEllipse(
                    null,
                    (Pen)style.NativeStroke(),
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
                    (Pen)style.NativeStroke(),
                    new Point(
                        position.InvertX2,
                        position.InvertY2),
                    InvertSize,
                    InvertSize);
            }

            double thickness = style.Thickness;
            double half = thickness / 2.0;
            var gs = new GuidelineSet(
                new double[] { half, half },
                new double[] { half, half });
            (dc as DrawingContext).PushGuidelineSet(gs);

            (dc as DrawingContext).DrawLine(
                (Pen)style.NativeStroke(),
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
