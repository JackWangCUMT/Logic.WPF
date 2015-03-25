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
    public class LogicPageTemplate : ITemplate
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public IContainer Grid { get; set; }
        public IContainer Table { get; set; }
        public IContainer Frame { get; set; }

        public LogicPageTemplate()
        {
            this.Name = "Logic Page";

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

            // styles
            var gridStyle = new XStyle()
            {
                Name = "Grid",
                Fill = new XColor() { A = 0x00, R = 0x00, G = 0x00, B = 0x00 },
                Stroke = new XColor() { A = 0xFF, R = 0xD3, G = 0xD3, B = 0xD3 },
                Thickness = 1.0
            };
            this.Grid.Styles.Add(gridStyle);

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

            // grid
            var settings = new GridFactory.GridSettings()
            {
                StartX = 330.0,
                StartY = 30.0,
                Width = 600.0,
                Height = 750.0,
                SizeX = 30.0,
                SizeY = 30.0
            };
            GridFactory.Create(this.Grid.Shapes, gridStyle, settings);

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
            // headers
            shapes.Add(
                new XText()
                {
                    X = 0.0,
                    Y = 0.0,
                    Width = 330.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 19.0,
                    Text = "I N P U T S",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 30.0 + 5.0,
                    Y = 30.0 + 0.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Designation",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 30.0 + 5.0,
                    Y = 30.0 + 15.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Description",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 30.0 + 215.0,
                    Y = 30.0 + 0.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Signal",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 30.0 + 215.0,
                    Y = 30.0 + 15.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Condition",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 330.0,
                    Y = 0.0,
                    Width = 600.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 19.0,
                    Text = "F U N C T I O N",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 930.0,
                    Y = 0.0,
                    Width = 330.0,
                    Height = 30.0,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 19.0,
                    Text = "O U T P U T S",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 930.0 + 5.0,
                    Y = 30.0 + 0.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Designation",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 930.0 + 5.0,
                    Y = 30.0 + 15.0,
                    Width = 200.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Description",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 930.0 + 215.0,
                    Y = 30.0 + 0.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Signal",
                    Style = style
                });
            shapes.Add(
                new XText()
                {
                    X = 930.0 + 215.0,
                    Y = 30.0 + 15.0,
                    Width = 80.0,
                    Height = 15.0,
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 11.0,
                    Text = "Condition",
                    Style = style
                });

            // numbers
            double lx = 0.0;
            double ly = 60.0;
            double rx = 1230.0;
            double ry = 60.0;
            for (int n = 1; n <= 24; n++)
            {
                shapes.Add(
                    new XText()
                    {
                        X = lx,
                        Y = ly,
                        Width = 30.0,
                        Height = 30.0,
                        HAlignment = HAlignment.Center,
                        VAlignment = VAlignment.Center,
                        FontName = "Consolas",
                        FontSize = 15.0,
                        Text = n.ToString("00"),
                        Style = style
                    });
                shapes.Add(
                    new XText()
                    {
                        X = rx,
                        Y = ry,
                        Width = 30.0,
                        Height = 30.0,
                        HAlignment = HAlignment.Center,
                        VAlignment = VAlignment.Center,
                        FontName = "Consolas",
                        FontSize = 15.0,
                        Text = n.ToString("00"),
                        Style = style
                    });
                ly += 30.0;
                ry += 30.0;
            }

            shapes.Add(new XLine() { X1 = 0.0, Y1 = 0.0, X2 = 1260.0, Y2 = 0.0, Style = style });
            shapes.Add(new XLine() { X1 = 0.0, Y1 = 30.0, X2 = 1260.0, Y2 = 30.0, Style = style });
            shapes.Add(new XLine() { X1 = 0.0, Y1 = 780.0, X2 = 1260.0, Y2 = 780.0, Style = style });
            shapes.Add(new XLine() { X1 = 0.0, Y1 = 811.0, X2 = 1260.0, Y2 = 811.0, Style = style });
            shapes.Add(new XLine() { X1 = 0.0, Y1 = 891.0, X2 = 1260.0, Y2 = 891.0, Style = style });

            shapes.Add(new XLine() { X1 = 0.0, Y1 = 0.0, X2 = 0.0, Y2 = 891.0, Style = style });
            shapes.Add(new XLine() { X1 = 30.0, Y1 = 30.0, X2 = 30.0, Y2 = 780.0, Style = style });
            shapes.Add(new XLine() { X1 = 240.0, Y1 = 30.0, X2 = 240.0, Y2 = 780.0, Style = style });
            shapes.Add(new XLine() { X1 = 330.0, Y1 = 0.0, X2 = 330.0, Y2 = 780.0, Style = style });

            shapes.Add(new XLine() { X1 = 930.0, Y1 = 0.0, X2 = 930.0, Y2 = 780.0, Style = style });
            shapes.Add(new XLine() { X1 = 1140.0, Y1 = 30.0, X2 = 1140.0, Y2 = 780.0, Style = style });
            shapes.Add(new XLine() { X1 = 1230.0, Y1 = 30.0, X2 = 1230.0, Y2 = 780.0, Style = style });
            shapes.Add(new XLine() { X1 = 1260.0, Y1 = 0.0, X2 = 1260.0, Y2 = 891.0, Style = style });

            for (double y = 60.0; y < 60.0 + 25.0 * 30.0; y += 30.0)
            {
                shapes.Add(new XLine() { X1 = 0.0, Y1 = y, X2 = 330.0, Y2 = y, Style = style });
                shapes.Add(new XLine() { X1 = 930.0, Y1 = y, X2 = 1260.0, Y2 = y, Style = style });
            }
        }
    }
}
