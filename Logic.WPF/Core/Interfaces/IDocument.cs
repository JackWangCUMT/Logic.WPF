// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IDocument
    {
        string Name { get; set; }
        bool IsActive { get; set; }
        IList<IPage> Pages { get; set; }
    }
}
