using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface ILayer
    {
        Func<bool> IsMouseCaptured { get; set; }
        Action CaptureMouse { get; set; }
        Action ReleaseMouseCapture { get; set; }
        Action InvalidateVisual { get; set; }
        void MouseLeftButtonDown(Point2 point);
        void MouseLeftButtonUp(Point2 point);
        void MouseMove(Point2 point);
        void MouseRightButtonDown(Point2 point);
        void Cancel();
        void OnRender(object dc);
    }
}
