// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IRenderer : IDisposable
    {
        IList<KeyValuePair<string, IProperty>> Database { get; set; }
        ICollection<IShape> Selected { get; set; }
        double Zoom { get; set; }
        double InvertSize { get; set; }
        double PinRadius { get; set; }
        double HitTreshold { get; set; }
        bool ShortenWire { get; set; }
        double ShortenSize { get; set; }
        void DrawLine(object dc, IStyle style, XLine line);
        void DrawEllipse(object dc, IStyle style, XEllipse ellipse);
        void DrawRectangle(object dc, IStyle style, XRectangle rectangle);
        void DrawText(object dc, IStyle style, XText text);
        void DrawImage(object dc, IStyle style, XImage image);
        void DrawPin(object dc, IStyle style, XPin pin);
        void DrawWire(object dc, IStyle style, XWire wire);
    }
}
