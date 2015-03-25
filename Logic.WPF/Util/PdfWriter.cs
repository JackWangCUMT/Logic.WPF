// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core = Logic.Core;

namespace Logic.Util
{
    public class PdfWriter : Core.IRenderer
    {
        #region Properties

        public bool EnablePinRendering { get; set; }
        public bool EnableGridRendering { get; set; }
        public Core.IStyle TemplateStyleOverride { get; set; }
        public Core.IStyle LayerStyleOverride { get; set; }

        #endregion

        #region Fields

        private Func<double, double> ScaleToPage;

        #endregion

        #region Create

        public void Create(string path, Core.IPage page)
        {
            using (var pdfDocument = new PdfDocument())
            {
                Add(pdfDocument, page);
                pdfDocument.Save(path);
            }
        }

        public void Create(string path, IEnumerable<Core.IPage> pages)
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

        private void Add(PdfDocument pdfDocument, Core.IPage page)
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

                // set scaling function
                ScaleToPage = (value) => value * scale;

                // set renderer database
                Database = page.Database;

                // draw block contents to pdf graphics
                RenderPage(gfx, page);

                // reset renderer database
                Database = null;
            }
        }

        #endregion

        #region Render

        private void RenderPage(object gfx, Core.IPage page)
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

        private void RenderTemplate(object gfx, Core.ITemplate template)
        {
            if (EnableGridRendering)
            {
                RenderConatiner(gfx, template.Grid);
            }

            RenderConatiner(gfx, template.Table);
            RenderConatiner(gfx, template.Frame);
        }

        private void RenderConatiner(object gfx, Core.IContainer container)
        {
            Core.IStyle overrideStyle = TemplateStyleOverride;

            foreach (var shape in container.Shapes)
            {
                shape.Render(
                    gfx, 
                    this, 
                    overrideStyle == null ? shape.Style : overrideStyle);
            }
        }

        private void RenderLayer(object gfx, IEnumerable<Core.IShape> shapes)
        {
            Core.IStyle overrideStyle = LayerStyleOverride;

            foreach (var shape in shapes)
            {
                shape.Render(
                    gfx, 
                    this, 
                    overrideStyle == null ? shape.Style : overrideStyle);

                if (EnablePinRendering)
                {
                    if (shape is Core.XBlock)
                    {
                        foreach (var pin in (shape as Core.XBlock).Pins)
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

        private XColor ToXColor(Core.IColor color)
        {
            return XColor.FromArgb(
                color.A, 
                color.R, 
                color.G, 
                color.B);
        }

        private XPen ToXPen(Core.IStyle style)
        {
            return new XPen(
                ToXColor(style.Stroke), 
                ScaleToPage(XUnit.FromMillimeter(style.Thickness).Value))
            {
                LineCap = XLineCap.Round
            };
        }

        private XSolidBrush ToXSolidBrush(Core.IColor color)
        {
            return new XSolidBrush(ToXColor(color));
        } 

        #endregion

        #region IRenderer

        public IList<KeyValuePair<string, Core.IProperty>> Database { get; set; }
        public ICollection<Core.IShape> Selected { get; set; }
        public double Zoom { get; set; }
        public double InvertSize { get; set; }
        public double PinRadius { get; set; }
        public double HitTreshold { get; set; }
        public bool ShortenWire { get; set; }
        public double ShortenSize { get; set; }

        public void DrawLine(object gfx, Core.IStyle style, Core.XLine line)
        {
            (gfx as XGraphics).DrawLine(
                ToXPen(style), 
                ScaleToPage(line.X1), 
                ScaleToPage(line.Y1), 
                ScaleToPage(line.X2), 
                ScaleToPage(line.Y2));
        }

        public void DrawEllipse(object gfx, Core.IStyle style, Core.XEllipse ellipse)
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

        public void DrawRectangle(object gfx, Core.IStyle style, Core.XRectangle rectangle)
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

        public void DrawText(object gfx, Core.IStyle style, Core.XText text)
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
                case Core.HAlignment.Left: 
                    format.Alignment = XStringAlignment.Near; 
                    break;
                case Core.HAlignment.Center: 
                    format.Alignment = XStringAlignment.Center; 
                    break;
                case Core.HAlignment.Right: 
                    format.Alignment = XStringAlignment.Far; 
                    break;
            }

            switch (text.VAlignment)
            {
                case Core.VAlignment.Top: 
                    format.LineAlignment = XLineAlignment.Near; 
                    break;
                case Core.VAlignment.Center: 
                    format.LineAlignment = XLineAlignment.Center; 
                    break;
                case Core.VAlignment.Bottom: 
                    format.LineAlignment = XLineAlignment.Far; 
                    break;
            }

            if (text.IsFilled)
            {
                (gfx as XGraphics).DrawRectangle(ToXSolidBrush(style.Fill), rect);
            }

            (gfx as XGraphics).DrawString(
                text.Bind(Database), 
                font, 
                ToXSolidBrush(style.Stroke), 
                rect, 
                format);
        }

        public void DrawImage(object gfx, Core.IStyle style, Core.XImage image)
        {
            (gfx as XGraphics).DrawImage(
                XImage.FromFile(image.Path.LocalPath),
                ScaleToPage(image.X),
                ScaleToPage(image.Y),
                ScaleToPage(image.Width),
                ScaleToPage(image.Height));
        }

        public void DrawPin(object gfx, Core.IStyle style, Core.XPin pin)
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

        public void DrawWire(object gfx, Core.IStyle style, Core.XWire wire)
        {
            var position = WirePosition.Calculate(
                wire, 
                InvertSize,
                ShortenWire,
                ShortenSize);

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

        #region IDisposable

        public void Dispose() { }

        #endregion
    }
}
