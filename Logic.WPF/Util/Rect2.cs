using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public struct Rect2
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
        public double Left { get { return X; } }
        public double Right { get { return Width + X; } }
        public double Top { get { return Y; } }
        public double Bottom { get { return Y + Height; } }

        public Rect2(double x, double y, double width, double height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public bool Contains(Point2 point)
        {
            return ((point.X >= X)
                && (point.X - Width <= X)
                && (point.Y >= Y)
                && (point.Y - Height <= Y));
        }

        public bool IntersectsWith(Rect2 rect)
        {
            return (rect.Left <= Right)
                && (rect.Right >= Left)
                && (rect.Top <= Bottom)
                && (rect.Bottom >= Top);
        }
    }
}
