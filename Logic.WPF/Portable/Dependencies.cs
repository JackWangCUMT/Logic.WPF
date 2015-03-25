// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Portable
{
    public class Dependencies
    {
        public ITextClipboard TextClipboard { get; set; }
        public IRenderer Renderer { get; set; }
        public ICurrentApplication CurrentApplication { get; set; }
        public IFileDialog FileDialog { get; set; }
        public IMainView MainView { get; set; }
    }
}
