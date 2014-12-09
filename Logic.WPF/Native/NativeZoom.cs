using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Logic.Native
{
    public class NativeZoom : Border
    {
        public Action<double> InvalidateChild { get; set; }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                {
                    UIElement child = value;
                    Point origin = new Point();
                    Point start = new Point();

                    var group = new TransformGroup();
                    var st = new ScaleTransform();
                    group.Children.Add(st);
                    var tt = new TranslateTransform();
                    group.Children.Add(tt);

                    child.RenderTransform = group;
                    child.RenderTransformOrigin = new Point(0.0, 0.0);

                    this.MouseWheel += (s, e) =>
                    {
                        if (child != null)
                        {
                            double zoom = e.Delta > 0 ? .2 : -.2;
                            if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                                return;

                            Point relative = e.GetPosition(child);
                            double abosuluteX = relative.X * st.ScaleX + tt.X;
                            double abosuluteY = relative.Y * st.ScaleY + tt.Y;
                            st.ScaleX += zoom;
                            st.ScaleY += zoom;
                            tt.X = abosuluteX - relative.X * st.ScaleX;
                            tt.Y = abosuluteY - relative.Y * st.ScaleY;

                            if (InvalidateChild != null)
                            {
                                InvalidateChild(st.ScaleX);
                            }
                        }
                    };

                    this.MouseMove += (s, e) =>
                    {
                        if (child != null && child.IsMouseCaptured)
                        {
                            Vector v = start - e.GetPosition(this);
                            tt.X = origin.X - v.X;
                            tt.Y = origin.Y - v.Y;
                        }
                    };

                    this.MouseDown += (s, e) =>
                    {
                        if (child != null
                            && e.ChangedButton == MouseButton.Middle
                            && e.ClickCount == 1
                            && e.ButtonState == MouseButtonState.Pressed)
                        {
                            start = e.GetPosition(this);
                            origin = new Point(tt.X, tt.Y);
                            this.Cursor = Cursors.Hand;
                            child.CaptureMouse();
                        }

                        if (child != null
                            && e.ChangedButton == MouseButton.Middle
                            && e.ClickCount == 2)
                        {
                            st.ScaleX = 1.0;
                            st.ScaleY = 1.0;
                            tt.X = 0.0;
                            tt.Y = 0.0;

                            if (InvalidateChild != null)
                            {
                                InvalidateChild(st.ScaleX);
                            }
                        }
                    };

                    this.MouseUp += (s, e) =>
                    {
                        if (child != null
                            && e.ChangedButton == MouseButton.Middle
                            && e.ClickCount == 1
                            && e.ButtonState == MouseButtonState.Released)
                        {
                            child.ReleaseMouseCapture();
                            this.Cursor = Cursors.Arrow;
                        }
                    };
                }

                base.Child = value;
            }
        }
    }
}
