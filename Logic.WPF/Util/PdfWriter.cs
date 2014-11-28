using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE = Logic.Core;
using PAGE = Logic.Page;

namespace Logic.Util
{
    internal class XPdfStyle : CORE.IStyle
    {
        public string Name { get; set; }
        public CORE.IColor Fill { get; set; }
        public CORE.IColor Stroke { get; set; }
        public double Thickness { get; set; }

        public object NativeFill() { return null; }
        public object NativeStroke() { return null; }

        public XPdfStyle() { }

        public XPdfStyle(
            string name, 
            CORE.XColor fill, 
            CORE.XColor stroke, 
            double thickness)
        {
            Name = name;
            Fill = fill;
            Stroke = stroke;
            Thickness = thickness;
        }
    }

    public class PdfWriter : CORE.IRenderer
    {
        #region Properties

        public bool EnablePinRendering { get; set; }
        public bool EnableGridRendering { get; set; }
        public CORE.IStyle TemplateStyleOverride { get; set; }
        public CORE.IStyle LayerStyleOverride { get; set; }

        #endregion

        #region Fields

        private Func<double, double> ScaleToPage;

        #endregion

        #region Create

        public void Create(string path, CORE.XPage page)
        {
            using (var document = new PdfDocument())
            {
                Add(document, page);
                document.Save(path);
            }
        }

        public void Create(string path, IEnumerable<CORE.XPage> pages)
        {
            using (var pdfDocument = new PdfDocument())
            {
                foreach (var page in pages)
                {
                    Add(pdfDocument, page);
                }
                pdfDocument.Save(path);
            }
        }

        private void Add(PdfDocument pdfDocument, CORE.XPage page)
        {
            // create A4 page with landscape orientation
            PdfPage pdfPage = pdfDocument.AddPage();
            pdfPage.Size = PageSize.A4;
            pdfPage.Orientation = PageOrientation.Landscape;

            using (XGraphics gfx = XGraphics.FromPdfPage(pdfPage))
            {
                // calculate x and y page scale factors
                double scaleX = pdfPage.Width.Value / page.Template.Width;
                double scaleY = pdfPage.Height.Value / page.Template.Height;
                double scale = Math.Min(scaleX, scaleY);

                ScaleToPage = (value) => value * scale;

                // draw block contents to pdf graphics
                RenderPage(gfx, page);
            }
        }

        #endregion

        #region Render

        private void RenderPage(object gfx, CORE.XPage page)
        {
            RenderTemplate(gfx, page.Template);

            RenderLayer(gfx, page.Shapes);
            RenderLayer(gfx, page.Blocks);

            if (EnablePinRendering)
            {
                RenderLayer(gfx, page.Pins);
            }

            RenderLayer(gfx, page.Wires);
        }

        private void RenderTemplate(object gfx, CORE.ITemplate template)
        {
            if (EnableGridRendering)
            {
                RenderConatiner(gfx, template.Grid);
            }

            RenderConatiner(gfx, template.Table);
            RenderConatiner(gfx, template.Frame);
        }

        private void RenderConatiner(object gfx, CORE.XContainer container)
        {
            CORE.IStyle overrideStyle = TemplateStyleOverride;

            foreach (var shape in container.Shapes)
            {
                shape.Render(
                    gfx, 
                    this, 
                    overrideStyle == null ? shape.Style : overrideStyle);
            }
        }

        private void RenderLayer(object gfx, IEnumerable<CORE.IShape> shapes)
        {
            CORE.IStyle overrideStyle = LayerStyleOverride;

            foreach (var shape in shapes)
            {
                shape.Render(
                    gfx, 
                    this, 
                    overrideStyle == null ? shape.Style : overrideStyle);

                if (EnablePinRendering)
                {
                    if (shape is CORE.XBlock)
                    {
                        foreach (var pin in (shape as CORE.XBlock).Pins)
                        {
                            pin.Render(
                                gfx, 
                                this, 
                                overrideStyle == null ? pin.Style : overrideStyle);
                        }
                    }
                }
            }
        }

        #endregion

        #region Helpers

        private XColor ToXColor(CORE.IColor color)
        {
            return XColor.FromArgb(
                color.A, 
                color.R, 
                color.G, 
                color.B);
        }

        private XPen ToXPen(CORE.IStyle style)
        {
            return new XPen(
                ToXColor(style.Stroke), 
                ScaleToPage(XUnit.FromMillimeter(style.Thickness).Value))
            {
                LineCap = XLineCap.Round
            };
        }

        private XSolidBrush ToXSolidBrush(CORE.IColor color)
        {
            return new XSolidBrush(ToXColor(color));
        } 

        #endregion

        #region IRenderer

        public ICollection<CORE.IShape> Selected { get; set; }
        public double InvertSize { get; set; }
        public double PinRadius { get; set; }
        public double HitTreshold { get; set; }

        public void DrawLine(object gfx, CORE.IStyle style, CORE.XLine line)
        {
            (gfx as XGraphics).DrawLine(
                ToXPen(style), 
                ScaleToPage(line.X1), 
                ScaleToPage(line.Y1), 
                ScaleToPage(line.X2), 
                ScaleToPage(line.Y2));
        }

        public void DrawEllipse(object gfx, CORE.IStyle style, CORE.XEllipse ellipse)
        {
            double x = ellipse.X - ellipse.RadiusX;
            double y = ellipse.Y - ellipse.RadiusY;
            double width = ellipse.RadiusX + ellipse.RadiusX;
            double height = ellipse.RadiusY + ellipse.RadiusY;

            if (ellipse.IsFilled)
            {
                (gfx as XGraphics).DrawEllipse(
                    ToXPen(style), 
                    ToXSolidBrush(style.Fill), 
                    ScaleToPage(x), 
                    ScaleToPage(y), 
                    ScaleToPage(width), 
                    ScaleToPage(height));
            }
            else
            {
                (gfx as XGraphics).DrawEllipse(
                    ToXPen(style),
                    ScaleToPage(x),
                    ScaleToPage(y),
                    ScaleToPage(width),
                    ScaleToPage(height));
            }
        }

        public void DrawRectangle(object gfx, CORE.IStyle style, CORE.XRectangle rectangle)
        {
            if (rectangle.IsFilled)
            {
                (gfx as XGraphics).DrawRectangle(
                    ToXPen(style),
                    ToXSolidBrush(style.Fill),
                    ScaleToPage(rectangle.X),
                    ScaleToPage(rectangle.Y),
                    ScaleToPage(rectangle.Width),
                    ScaleToPage(rectangle.Height));
            }
            else
            {
                (gfx as XGraphics).DrawRectangle(
                    ToXPen(style),
                    ScaleToPage(rectangle.X),
                    ScaleToPage(rectangle.Y),
                    ScaleToPage(rectangle.Width),
                    ScaleToPage(rectangle.Height));
            }
        }

        public void DrawText(object gfx, CORE.IStyle style, CORE.XText text)
        {
            XPdfFontOptions options = new XPdfFontOptions(
                PdfFontEncoding.Unicode, 
                PdfFontEmbedding.Always);

            XFont font = new XFont(
                text.FontName, 
                ScaleToPage(text.FontSize), 
                XFontStyle.Regular,
                options);

            XRect rect = new XRect(
                ScaleToPage(text.X), 
                ScaleToPage(text.Y), 
                ScaleToPage(text.Width), 
                ScaleToPage(text.Height));

            XStringFormat format = new XStringFormat();
            switch (text.HAlignment)
            {
                case CORE.HAlignment.Left: 
                    format.Alignment = XStringAlignment.Near; 
                    break;
                case CORE.HAlignment.Center: 
                    format.Alignment = XStringAlignment.Center; 
                    break;
                case CORE.HAlignment.Right: 
                    format.Alignment = XStringAlignment.Far; 
                    break;
            }

            switch (text.VAlignment)
            {
                case CORE.VAlignment.Top: 
                    format.LineAlignment = XLineAlignment.Near; 
                    break;
                case CORE.VAlignment.Center: 
                    format.LineAlignment = XLineAlignment.Center; 
                    break;
                case CORE.VAlignment.Bottom: 
                    format.LineAlignment = XLineAlignment.Far; 
                    break;
            }

            if (text.IsFilled)
            {
                (gfx as XGraphics).DrawRectangle(ToXSolidBrush(style.Fill), rect);
            }

            (gfx as XGraphics).DrawString(
                (text.Properties != null) ? string.Format(text.Text, text.Properties) : text.Text, 
                font, 
                ToXSolidBrush(style.Stroke), 
                rect, 
                format);
        }

        public void DrawPin(object gfx, CORE.IStyle style, CORE.XPin pin)
        {
            double x = pin.X - PinRadius;
            double y = pin.Y - PinRadius;
            double width = PinRadius + PinRadius;
            double height = PinRadius + PinRadius;

            (gfx as XGraphics).DrawEllipse(
                ToXPen(style), 
                ToXSolidBrush(style.Fill), 
                ScaleToPage(x), 
                ScaleToPage(y), 
                ScaleToPage(width), 
                ScaleToPage(height));
        }

        public void DrawWire(object gfx, CORE.IStyle style, CORE.XWire wire)
        {
            var position = PAGE.XWirePosition.Calculate(wire, InvertSize);

            if (wire.InvertStart)
            {
                double x = position.InvertX1 - InvertSize;
                double y = position.InvertY1 - InvertSize;
                double width = InvertSize + InvertSize;
                double height = InvertSize + InvertSize;

                (gfx as XGraphics).DrawEllipse(
                    ToXPen(style),
                    ScaleToPage(x),
                    ScaleToPage(y),
                    ScaleToPage(width),
                    ScaleToPage(height));
            }

            if (wire.InvertEnd)
            {
                double x = position.InvertX2 - InvertSize;
                double y = position.InvertY2 - InvertSize;
                double width = InvertSize + InvertSize;
                double height = InvertSize + InvertSize;

                (gfx as XGraphics).DrawEllipse(
                    ToXPen(style),
                    ScaleToPage(x),
                    ScaleToPage(y),
                    ScaleToPage(width),
                    ScaleToPage(height));
            }

            (gfx as XGraphics).DrawLine(
                ToXPen(style),
                ScaleToPage(position.StartX),
                ScaleToPage(position.StartY),
                ScaleToPage(position.EndX),
                ScaleToPage(position.EndY));
        } 

        #endregion
    }
}
