using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Logic.Util
{
    public static class NativePointExtensions
    {
        public static Point2 ToPoint1(this Point point)
        {
            return new Point2(point.X, point.Y);
        }
    }
}
