// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.Native;
using Logic.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Templates
{
    public class TitlePageTemplate : ITemplate
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public IContainer Grid { get; set; }
        public IContainer Table { get; set; }
        public IContainer Frame { get; set; }

        public TitlePageTemplate()
        {
            this.Name = "Title Page";

            this.Width = 1260.0;
            this.Height = 891.0;

            // containers
            this.Grid = new XContainer()
            {
                Styles = new ObservableCollection<IStyle>(),
                Shapes = new ObservableCollection<IShape>()
            };

            this.Table = new XContainer()
            {
                Styles = new ObservableCollection<IStyle>(),
                Shapes = new ObservableCollection<IShape>()
            };

            this.Frame = new XContainer()
            {
                Styles = new ObservableCollection<IStyle>(),
                Shapes = new ObservableCollection<IShape>()
            };

            var tableStyle = new XStyle()
            {
                Name = "Table",
                Fill = new XColor() { A = 0x00, R = 0x00, G = 0x00, B = 0x00 },
                Stroke = new XColor() { A = 0xFF, R = 0xD3, G = 0xD3, B = 0xD3 },
                Thickness = 1.0
            };
            this.Table.Styles.Add(tableStyle);

            var frameStyle = new XStyle()
            {
                Name = "Frame",
                Fill = new XColor() { A = 0x00, R = 0x00, G = 0x00, B = 0x00 },
                Stroke = new XColor() { A = 0xFF, R = 0xA9, G = 0xA9, B = 0xA9 },
                Thickness = 1.0
            };
            this.Frame.Styles.Add(frameStyle);

            // table
            CreateTable(this.Table.Shapes, tableStyle);

            // frame
            CreateFrame(this.Frame.Shapes, frameStyle);
        }

        private void CreateTable(IList<IShape> shapes, IStyle style)
        {
            double sx = 0.0;
            double sy = 811.0;

            shapes.Add(new XLine() { X1 = sx + 30, Y1 = sy + 0.0, X2 = sx + 30, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 75, Y1 = sy + 0.0, X2 = sx + 75, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 0, Y1 = sy + 20.0, X2 = sx + 175, Y2 = sy + 20.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 0, Y1 = sy + 40.0, X2 = sx + 175, Y2 = sy + 40.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 0, Y1 = sy + 60.0, X2 = sx + 175, Y2 = sy + 60.0, Style = style });

            shapes.Add(new XLine() { X1 = sx + 175, Y1 = sy + 0.0, X2 = sx + 175, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 290, Y1 = sy + 0.0, X2 = sx + 290, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 405, Y1 = sy + 0.0, X2 = sx + 405, Y2 = sy + 80.0, Style = style });

            shapes.Add(new XLine() { X1 = sx + 405, Y1 = sy + 20.0, X2 = sx + 1260, Y2 = sy + 20.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 405, Y1 = sy + 40.0, X2 = sx + 695, Y2 = sy + 40.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 965, Y1 = sy + 40.0, X2 = sx + 1260, Y2 = sy + 40.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 405, Y1 = sy + 60.0, X2 = sx + 695, Y2 = sy + 60.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 965, Y1 = sy + 60.0, X2 = sx + 1260, Y2 = sy + 60.0, Style = style });

            shapes.Add(new XLine() { X1 = sx + 465, Y1 = sy + 0.0, X2 = sx + 465, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 595, Y1 = sy + 0.0, X2 = sx + 595, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 640, Y1 = sy + 0.0, X2 = sx + 640, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 695, Y1 = sy + 0.0, X2 = sx + 695, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 965, Y1 = sy + 0.0, X2 = sx + 965, Y2 = sy + 80.0, Style = style });

            shapes.Add(new XLine() { X1 = sx + 1005, Y1 = sy + 0.0, X2 = sx + 1005, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 1045, Y1 = sy + 0.0, X2 = sx + 1045, Y2 = sy + 80.0, Style = style });
            shapes.Add(new XLine() { X1 = sx + 1100, Y1 = sy + 0.0, X2 = sx + 1100, Y2 = sy + 80.0, Style = style });
        }

        private void CreateFrame(IList<IShape> shapes, IStyle style)
        {
            // main title
            shapes.Add(
                new XText()
                {
                    X = 200.0,
                    Y = 200.0,
                    Width = 1260.0 - 200.0 - 200.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 24.0,
                    Text = "MAIN TITLE",
                    TextBinding = "MainTitle",
                    Style = style
                });

            // sub title
            shapes.Add(
                new XText()
                {
                    X = 200.0,
                    Y = 200.0 + 30.0,
                    Width = 1260.0 - 200.0 - 200.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 24.0,
                    Text = "SUB TITLE",
                    TextBinding = "SubTitle",
                    Style = style
                });

            shapes.Add(new XLine() { X1 = 0.0, Y1 = 0.0, X2 = 1260.0, Y2 = 0.0, Style = style });
            shapes.Add(new XLine() { X1 = 0.0, Y1 = 780.0, X2 = 1260.0, Y2 = 780.0, Style = style });
            shapes.Add(new XLine() { X1 = 0.0, Y1 = 811.0, X2 = 1260.0, Y2 = 811.0, Style = style });
            shapes.Add(new XLine() { X1 = 0.0, Y1 = 891.0, X2 = 1260.0, Y2 = 891.0, Style = style });

            shapes.Add(new XLine() { X1 = 0.0, Y1 = 0.0, X2 = 0.0, Y2 = 891.0, Style = style });
            shapes.Add(new XLine() { X1 = 1260.0, Y1 = 0.0, X2 = 1260.0, Y2 = 891.0, Style = style });
        }
    }
}
