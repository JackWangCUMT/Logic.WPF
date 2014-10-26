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
    public class XTable : Canvas
    {
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var pen = new Pen(Brushes.LightGray, 1.0);
            var gs = new GuidelineSet(
                new double[] { 0.5, 0.5 }, 
                new double[] { 0.5, 0.5 });
            dc.PushGuidelineSet(gs);

            double sx = 0.0;
            double sy = 811.0;

            dc.DrawLine(pen, new Point(sx + 30, sy + 0.0), new Point(sx + 30, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 75, sy + 0.0), new Point(sx + 75, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 0, sy + 20.0), new Point(sx + 175, sy + 20.0));
            dc.DrawLine(pen, new Point(sx + 0, sy + 40.0), new Point(sx + 175, sy + 40.0));
            dc.DrawLine(pen, new Point(sx + 0, sy + 60.0), new Point(sx + 175, sy + 60.0));

            dc.DrawLine(pen, new Point(sx + 175, sy + 0.0), new Point(sx + 175, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 290, sy + 0.0), new Point(sx + 290, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 405, sy + 0.0), new Point(sx + 405, sy + 80.0));

            dc.DrawLine(pen, new Point(sx + 405, sy + 20.0), new Point(sx + 1260, sy + 20.0));
            dc.DrawLine(pen, new Point(sx + 405, sy + 40.0), new Point(sx + 695, sy + 40.0));
            dc.DrawLine(pen, new Point(sx + 965, sy + 40.0), new Point(sx + 1260, sy + 40.0));
            dc.DrawLine(pen, new Point(sx + 405, sy + 60.0), new Point(sx + 695, sy + 60.0));
            dc.DrawLine(pen, new Point(sx + 965, sy + 60.0), new Point(sx + 1260, sy + 60.0));

            dc.DrawLine(pen, new Point(sx + 465, sy + 0.0), new Point(sx + 465, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 595, sy + 0.0), new Point(sx + 595, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 640, sy + 0.0), new Point(sx + 640, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 695, sy + 0.0), new Point(sx + 695, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 965, sy + 0.0), new Point(sx + 965, sy + 80.0));

            dc.DrawLine(pen, new Point(sx + 1005, sy + 0.0), new Point(sx + 1005, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 1045, sy + 0.0), new Point(sx + 1045, sy + 80.0));
            dc.DrawLine(pen, new Point(sx + 1100, sy + 0.0), new Point(sx + 1100, sy + 80.0));

            dc.Pop();
        }
    }
}
