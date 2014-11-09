using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public interface IRenderer
    {
        ICollection<IShape> Selected { get; set; }
        double InvertSize { get; set; }
        double PinRadius { get; set; }
        double HitTreshold { get; set; }
        void DrawBlock(object dc, IStyle style, XBlock block);
        void DrawLine(object dc, IStyle style, XLine line);
        void DrawEllipse(object dc, IStyle style, XEllipse ellipse);
        void DrawRectangle(object dc, IStyle style, XRectangle rectangle);
        void DrawText(object dc, IStyle style, XText text);
        void DrawPin(object dc, IStyle style, XPin pin);
        void DrawWire(object dc, IStyle style, XWire wire);
        void DrawSelection(object dc, IStyle style, XRectangle rectangle);
        void DrawShapes(object dc, IList<IShape> shapes);
        void DrawShapes(object dc, IStyle normal, IStyle selected, IList<IShape> shapes);
    }
}
