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
    public class XGrid : Canvas
    {
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double sx = 330.0;
            double sy = 30.0;
            double width = 600.0;
            double height = 750.0;
            double size = 30.0;

            var pen = new Pen(Brushes.LightGray, 1.0);
            var gs = new GuidelineSet(
                new double[] { 0.5, 0.5 }, 
                new double[] { 0.5, 0.5 });
            dc.PushGuidelineSet(gs);

            for (double x = sx + size; x < sx + width; x += size)
            {
                dc.DrawLine(pen, new Point(x, sy), new Point(x, sy + height));
            }

            for (double y = sy + size; y < sy + height; y += size)
            {
                dc.DrawLine(pen, new Point(sx, y), new Point(sx + width, y));
            }

            dc.Pop();
        }
    }
}
