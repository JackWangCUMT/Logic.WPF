using Logic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Logic.Page
{
    public interface ILayer
    {
        Func<bool> IsMouseCaptured { get; set; }
        Action CaptureMouse { get; set; }
        Action ReleaseMouseCapture { get; set; }
        Action InvalidateVisual { get; set; }
        void MouseLeftButtonDown(Point1 point);
        void MouseLeftButtonUp(Point1 point);
        void MouseMove(Point1 point);
        void MouseRightButtonDown(Point1 point);
        void OnRender(object dc);
    }
}
