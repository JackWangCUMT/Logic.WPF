// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
        void MouseCancel();
        void OnRender(object dc);
    }
}
