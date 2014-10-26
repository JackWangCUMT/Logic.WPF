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
    public class XFrame : Canvas
    {
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var pen = new Pen(Brushes.DarkGray, 1.0);
            var gs = new GuidelineSet(
                new double[] { 0.5, 0.5 }, 
                new double[] { 0.5, 0.5 });
            dc.PushGuidelineSet(gs);

            dc.DrawLine(pen, new Point(0.0, 0.0), new Point(1260.0, 0.0));
            dc.DrawLine(pen, new Point(0.0, 30.0), new Point(1260.0, 30.0));
            dc.DrawLine(pen, new Point(0.0, 780.0), new Point(1260.0, 780.0));
            dc.DrawLine(pen, new Point(0.0, 811.0), new Point(1260.0, 811.0));
            dc.DrawLine(pen, new Point(0.0, 891.0), new Point(1260.0, 891.0));

            dc.DrawLine(pen, new Point(0.0, 0.0), new Point(0.0, 891.0));
            dc.DrawLine(pen, new Point(30.0, 30.0), new Point(30.0, 780.0));
            dc.DrawLine(pen, new Point(240.0, 30.0), new Point(240.0, 780.0));
            dc.DrawLine(pen, new Point(330.0, 0.0), new Point(330.0, 780.0));

            dc.DrawLine(pen, new Point(930.0, 0.0), new Point(930.0, 780.0));
            dc.DrawLine(pen, new Point(1140.0, 30.0), new Point(1140.0, 780.0));
            dc.DrawLine(pen, new Point(1230.0, 30.0), new Point(1230.0, 780.0));
            dc.DrawLine(pen, new Point(1260.0, 0.0), new Point(1260.0, 891.0));

            for (double y = 60.0; y < 60.0 + 25.0 * 30.0; y += 30.0)
            {
                dc.DrawLine(pen, new Point(0.0, y), new Point(330.0, y));
                dc.DrawLine(pen, new Point(930.0, y), new Point(1260.0, y));
            }

            dc.Pop();
        }
    }
}
