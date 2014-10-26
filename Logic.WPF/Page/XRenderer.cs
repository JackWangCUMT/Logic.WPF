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
    public class XRenderer : IRenderer
    {
        public static double InvertSize = 6.0;
        public static double PinRadius = 4.0;
        public static double HitTreshold = 6.0;

        public void DrawBlock(object dc, XBlock block)
        {
            foreach (var shape in block.Shapes)
            {
                shape.Render(dc, this);
            }

            foreach (var pin in block.Pins)
            {
                pin.Render(dc, this);
            }
        }

        public void DrawLine(object dc, XLine line)
        {
            (dc as DrawingContext).DrawLine(
                new Pen(Brushes.Black, 2.0),
                new Point(line.X1, line.Y1),
                new Point(line.X2, line.Y2));
        }

        public void DrawEllipse(object dc, XEllipse ellipse)
        {
            (dc as DrawingContext).DrawEllipse(
                ellipse.IsFilled ? Brushes.Black : null,
                new Pen(Brushes.Black, 2.0),
                new Point(ellipse.X, ellipse.Y),
                ellipse.RadiusX,
                ellipse.RadiusY);
        }

        public void DrawRectangle(object dc, XRectangle rectangle)
        {
            (dc as DrawingContext).DrawRectangle(
                rectangle.IsFilled ? Brushes.Black : null,
                new Pen(Brushes.Black, 2.0),
                new Rect(
                    rectangle.X, 
                    rectangle.Y, 
                    rectangle.Width, 
                    rectangle.Height));
        }

        public void DrawText(object dc, XText text)
        {
            var ft = new FormattedText(
                text.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(text.FontName),
                text.FontSize,
                Brushes.Black,
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
                    Brushes.Yellow,
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

        public void DrawPin(object dc, XPin pin)
        {
            (dc as DrawingContext).DrawEllipse(
                Brushes.Black,
                new Pen(Brushes.Black, 2.0),
                new Point(pin.X, pin.Y),
                PinRadius,
                PinRadius);
        }

        public void DrawWire(object dc, XWire wire)
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
                    new Pen(Brushes.Black, 2.0),
                    new Point(ix1, iy1),
                    InvertSize,
                    InvertSize);
            }

            if (wire.InvertEnd)
            {
                (dc as DrawingContext).DrawEllipse(
                    null,
                    new Pen(Brushes.Black, 2.0),
                    new Point(ix2, iy2),
                    InvertSize,
                    InvertSize);
            }

            (dc as DrawingContext).DrawLine(
                new Pen(Brushes.Black, 2.0),
                new Point(x1, y1),
                new Point(x2, y2));
        }
    }
}
