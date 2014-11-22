using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Logic.WPF.Page
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
                Layer.MouseLeftButtonDown(e.GetPosition(this));
            };

            PreviewMouseLeftButtonUp += (s, e) =>
            {
                Layer.MouseLeftButtonUp(e.GetPosition(this));
            };

            PreviewMouseMove += (s, e) =>
            {
                Layer.MouseMove(e.GetPosition(this));
            };

            PreviewMouseRightButtonDown += (s, e) =>
            {
                Layer.MouseRightButtonDown(e.GetPosition(this));
            };
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            Layer.OnRender(dc);
        }
    }
}
