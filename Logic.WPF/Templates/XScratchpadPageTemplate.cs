using Logic.Core;
using Logic.WPF.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Templates
{
    public class XScratchpadPageTemplate : ITemplate
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public XContainer Grid { get; set; }
        public XContainer Table { get; set; }
        public XContainer Frame { get; set; }

        public XScratchpadPageTemplate()
        {
            this.Name = "Scratchpad";

            this.Width = 840;
            this.Height = 750;

            // containers
            this.Grid = new XContainer() 
            { 
                Styles = new List<IStyle>(),
                Shapes = new List<IShape>() 
            };

            this.Table = new XContainer()
            {
                Styles = new List<IStyle>(),
                Shapes = new List<IShape>()
            };

            this.Frame = new XContainer()
            {
                Styles = new List<IStyle>(),
                Shapes = new List<IShape>()
            };

            // styles
            var gridStyle = new XStyle(
                name: "Grid",
                fill: new XColor() { A = 0x00, R = 0x00, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0xD3, G = 0xD3, B = 0xD3 },
                thickness: 1.0);
            this.Grid.Styles.Add(gridStyle);

            // grid
            var options = new GridFactory.Options()
            {
                StartX = 30.0,
                StartY = 30.0,
                Width = 840.0,
                Height = 750.0,
                SizeX = 30.0,
                SizeY = 30.0
            };
            GridFactory.Create(this.Grid.Shapes, gridStyle, options);
        }
    }
}
