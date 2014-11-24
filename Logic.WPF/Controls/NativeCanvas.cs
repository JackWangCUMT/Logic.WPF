using Logic.Page;
using Logic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Logic.Controls
{
    public class NativeCanvas : Canvas
    {
        public XLayer Layer { get; private set; }

        public NativeCanvas()
        {
            InitializeLayer();
        }

        private void InitializeLayer()
        {
            Layer = new XLayer()
            {
                IsMouseCaptured = () => 
                {
                    return this.IsMouseCaptured;
                },
                CaptureMouse = () =>
                {
                    this.CaptureMouse();
                },
                ReleaseMouseCapture = () => 
                {
                    this.ReleaseMouseCapture();
                },
                InvalidateVisual = () => 
                {
                    this.InvalidateVisual();
                }
            };

            PreviewMouseLeftButtonDown += (s, e) =>
            {
                Layer.MouseLeftButtonDown(e.GetPosition(this).ToPoint1());
            };

            PreviewMouseLeftButtonUp += (s, e) =>
            {
                Layer.MouseLeftButtonUp(e.GetPosition(this).ToPoint1());
            };

            PreviewMouseMove += (s, e) =>
            {
                Layer.MouseMove(e.GetPosition(this).ToPoint1());
            };

            PreviewMouseRightButtonDown += (s, e) =>
            {
                Layer.MouseRightButtonDown(e.GetPosition(this).ToPoint1());
            };
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            Layer.OnRender(dc);
        }
    }
}
