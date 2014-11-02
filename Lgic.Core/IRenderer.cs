using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public interface IRenderer
    {
        void DrawBlock(object dc, XBlock block);
        void DrawLine(object dc, XLine line);
        void DrawEllipse(object dc, XEllipse ellipse);
        void DrawRectangle(object dc, XRectangle rectangle);
        void DrawText(object dc, XText text);
        void DrawPin(object dc, XPin pin);
        void DrawWire(object dc, XWire wire);
        void DrawSelection(object dc, XRectangle rectangle);
        void DrawShapes(object dc, IList<IShape> shapes);
    }
}
