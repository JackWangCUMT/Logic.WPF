// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface ITemplate
    {
        string Name { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        IContainer Grid { get; set; }
        IContainer Table { get; set; }
        IContainer Frame { get; set; }
    }
}
