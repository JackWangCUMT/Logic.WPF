// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IProject
    {
        string Name { get; set; }
        string DefaultTemplate { get; set; }
        IList<IStyle> Styles { get; set; }
        IList<ITemplate> Templates { get; set; }
        IList<IDocument> Documents { get; set; }
    }
}
