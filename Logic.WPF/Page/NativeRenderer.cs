using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Logic.WPF.Page
{
    public class NativeRenderer : IRenderer
    {
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
                text.Text,
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
            double x1, y1, x2, y2;

            if (wire.Start != null)
            {
                x1 = wire.Start.X;
                y1 = wire.Start.Y;
            }
            else
            {
                x1 = wire.X1;
                y1 = wire.Y1;
            }

            if (wire.End != null)
            {
                x2 = wire.End.X;
                y2 = wire.End.Y;
            }
            else
            {
                x2 = wire.X2;
                y2 = wire.Y2;
            }

            double ix1 = x1;
            double iy1 = y1;
            double ix2 = x2;
            double iy2 = y2;

            // vertical wire
            if (x1 == x2 && y1 != y2)
            {
                if (y1 < y2)
                {
                    if (wire.InvertStart)
                    {
                        y1 += 2 * InvertSize;
                        iy1 += InvertSize;
                    }

                    if (wire.InvertEnd)
                    {
                        y2 -= 2 * InvertSize;
                        iy2 -= InvertSize;
                    }
                }
                else
                {
                    if (wire.InvertStart)
                    {
                        y1 -= 2 * InvertSize;
                        iy1 -= InvertSize;
                    }

                    if (wire.InvertEnd)
                    {
                        y2 += 2 * InvertSize;
                        iy2 += InvertSize;
                    }
                }
            }

            // horizontal wire
            if (x1 != x2 && y1 == y2)
            {
                if (x1 < x2)
                {
                    if (wire.InvertStart)
                    {
                        x1 += 2 * InvertSize;
                        ix1 += InvertSize;
                    }

                    if (wire.InvertEnd)
                    {
                        x2 -= 2 * InvertSize;
                        ix2 -= InvertSize;
                    }
                }
                else
                {
                    if (wire.InvertStart)
                    {
                        x1 -= 2 * InvertSize;
                        ix1 -= InvertSize;
                    }

                    if (wire.InvertEnd)
                    {
                        x2 += 2 * InvertSize;
                        ix2 += InvertSize;
                    }
                }
            }

            if (wire.InvertStart)
            {
                (dc as DrawingContext).DrawEllipse(
                    null,
                    (Pen)style.NativeStroke(),
                    new Point(ix1, iy1),
                    InvertSize,
                    InvertSize);
            }

            if (wire.InvertEnd)
            {
                (dc as DrawingContext).DrawEllipse(
                    null,
                    (Pen)style.NativeStroke(),
                    new Point(ix2, iy2),
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
                new Point(x1, y1),
                new Point(x2, y2));

            (dc as DrawingContext).Pop();
        }
    }
}
