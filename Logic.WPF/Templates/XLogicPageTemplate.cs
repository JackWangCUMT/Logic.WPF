using Logic.Core;
using Logic.WPF.Page;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Templates
{
    public class XLogicPageTemplate : ITemplate
    {
        public string Name { get; set; }
        public XContainer Grid { get; set; }
        public XContainer Table { get; set; }
        public XContainer Frame { get; set; }

        public XLogicPageTemplate()
        {
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

            this.Name = "Logic Page";

            // styles
            var gridStyle = new XStyle(
                name: "Grid",
                fill: new XColor() { A = 0x00, R = 0x00, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0xD3, G = 0xD3, B = 0xD3 },
                thickness: 1.0);
            this.Grid.Styles.Add(gridStyle);

            var tableStyle = new XStyle(
                name: "Table",
                fill: new XColor() { A = 0x00, R = 0x00, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0xD3, G = 0xD3, B = 0xD3 },
                thickness: 1.0);
            this.Table.Styles.Add(tableStyle);

            var frameStyle = new XStyle(
                name: "Frame",
                fill: new XColor() { A = 0x00, R = 0x00, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0xA9, G = 0xA9, B = 0xA9 },
                thickness: 1.0);
            this.Frame.Styles.Add(frameStyle);

            // containers
            CreateGrid(this.Grid.Shapes, gridStyle);
            CreateTable(this.Table.Shapes, tableStyle);
            CreateFrame(this.Frame.Shapes, frameStyle);
        }

        private void CreateGrid(IList<IShape> shapes, IStyle style)
        {
            double sx = 330.0;
            double sy = 30.0;
            double width = 600.0;
            double height = 750.0;
            double size = 30.0;

            for (double x = sx + size; x < sx + width; x += size)
            {
                shapes.Add(new XLine() { X1 = x, Y1 = sy, X2 = x, Y2 = sy + height, Style = style });
            }

            for (double y = sy + size; y < sy + height; y += size)
            {
                shapes.Add(new XLine() { X1 = sx, Y1 = y, X2 = sx + width, Y2 = y, Style = style });
            }
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
