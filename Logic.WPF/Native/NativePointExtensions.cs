// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Logic.Native
{
    public static class NativePointExtensions
    {
        public static Point2 ToPoint2(this Point point)
        {
            return new Point2(point.X, point.Y);
        }
    }
}
