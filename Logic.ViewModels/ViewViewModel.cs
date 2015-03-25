// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.ViewModels
{
    public class ViewViewModel : ILayer
    {
        #region Properties

        public IRenderer Renderer { get; set; }
        public IContainer Container { get; set; }

        #endregion

        #region ILayer

        public Func<bool> IsMouseCaptured { get; set; }
        public Action CaptureMouse { get; set; }
        public Action ReleaseMouseCapture { get; set; }
        public Action InvalidateVisual { get; set; }

        public void MouseLeftButtonDown(Point2 point) { }

        public void MouseLeftButtonUp(Point2 point) { }

        public void MouseMove(Point2 point) { }

        public void MouseRightButtonDown(Point2 point) { }

        public void MouseCancel() { }

        public void OnRender(object dc)
        {
            if (Renderer != null)
            {
                var sw = Stopwatch.StartNew();

                if (Renderer != null
                   && Container != null
                   && Container.Shapes != null)
                {
                    foreach (var shape in Container.Shapes)
                    {
                        shape.Render(dc, Renderer, shape.Style);
                    }
                }
                sw.Stop();
                if (sw.Elapsed.TotalMilliseconds > (1000.0 / 60.0))
                {
                    Debug.WriteLine("View OnRender: " + sw.Elapsed.TotalMilliseconds + "ms");
                }
            }
        }

        #endregion
    }
}
