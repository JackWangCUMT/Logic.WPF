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
    public class ScratchpadPageTemplate : ITemplate
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public IContainer Grid { get; set; }
        public IContainer Table { get; set; }
        public IContainer Frame { get; set; }

        public ScratchpadPageTemplate()
        {
            this.Name = "Scratchpad";

            this.Width = 750;
            this.Height = 600;

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

            // grid
            var settings = new GridFactory.GridSettings()
            {
                StartX = 0.0,
                StartY = 0.0,
                Width = this.Width,
                Height = this.Height,
                SizeX = 30.0,
                SizeY = 30.0
            };
            GridFactory.Create(this.Grid.Shapes, gridStyle, settings);
        }
    }
}
