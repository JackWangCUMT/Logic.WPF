// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Logic.Native
{
    public class NativeGrid : Grid
    {
        public bool EnableAutoFit { get; set; }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (EnableAutoFit
                && base.Children != null
                && base.Children.Count == 1)
            {
                var zoom = base.Children[0] as NativeZoom;
                if (zoom != null
                    && zoom.AutoFitChild != null)
                {
                    zoom.AutoFitChild(
                        arrangeSize.Width,
                        arrangeSize.Height);
                }
            }
            return base.ArrangeOverride(arrangeSize);
        }
    }
}
