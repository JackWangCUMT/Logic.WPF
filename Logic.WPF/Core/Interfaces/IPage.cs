// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IPage
    {
        IList<KeyValuePair<string, IProperty>> Database { get; set; }
        string Name { get; set; }
        bool IsActive { get; set; }
        ITemplate Template { get; set; }
        IList<IShape> Shapes { get; set; }
        IList<IShape> Blocks { get; set; }
        IList<IShape> Pins { get; set; }
        IList<IShape> Wires { get; set; }
    }
}
