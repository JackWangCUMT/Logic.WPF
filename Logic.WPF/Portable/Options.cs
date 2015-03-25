// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Portable
{
    public class Options
    {
        public bool EnableLog { get; set; }
        public string LogPath { get; set; }

        public string PageName { get; set; }
        public string DocumentName { get; set; }
        public string ProjectName { get; set; }
        public string TemplateName { get; set; }

        public double InvertSize { get; set; }
        public double PinRadius { get; set; }
        public double HitTreshold { get; set; }
        public bool ShortenWire { get; set; }
        public double ShortenSize { get; set; }

        public bool EnableSnap { get; set; }
        public double SnapSize { get; set; }

        public bool EnableAutoFit { get; set; }

        public void Defaults()
        {
            // log
            EnableLog = true;
            LogPath = "Logic.WPF.log";

            // project
            PageName = "Page";
            DocumentName = "Document";
            ProjectName = "Project";
            TemplateName = "Scratchpad";

            // renderer
            InvertSize = 5.0;
            PinRadius = 4.0;
            HitTreshold = 6.0;
            ShortenWire = true;
            ShortenSize = 15.0;

            // layer
            EnableSnap = true;
            SnapSize = 15.0;

            // zoom
            EnableAutoFit = true;
        }

        public IPage EmptyPage()
        {
            return new XPage()
            {
                Database = new ObservableCollection<KeyValuePair<string, IProperty>>(),
                Name = PageName,
                Shapes = new ObservableCollection<IShape>(),
                Blocks = new ObservableCollection<IShape>(),
                Pins = new ObservableCollection<IShape>(),
                Wires = new ObservableCollection<IShape>(),
                Template = null
            };
        }

        public IDocument EmptyDocument()
        {
            return new XDocument()
            {
                Name = DocumentName,
                Pages = new ObservableCollection<IPage>()
            };
        }

        public IProject EmptyProject()
        {
            return new XProject()
            {
                Name = ProjectName,
                DefaultTemplate = TemplateName,
                Styles = new ObservableCollection<IStyle>(),
                Templates = new ObservableCollection<ITemplate>(),
                Documents = new ObservableCollection<IDocument>()
            };
        }

        public IPage EmptyTitlePage()
        {
            IPage page = EmptyPage();

            page.Database.Add(
                new KeyValuePair<string, IProperty>(
                    "MainTitle",
                    new XProperty("MAIN TITLE 1")));
            page.Database.Add(
                new KeyValuePair<string, IProperty>(
                    "SubTitle",
                    new XProperty("SUB TITLE 1")));

            return page;
        }
    }
}
