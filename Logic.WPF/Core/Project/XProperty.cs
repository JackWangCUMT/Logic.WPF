// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XProperty : IProperty
    {
        public XProperty() { }
        public XProperty(object data)
            : this()
        {
            this.Data = data;
        }
        public object Data { get; set; }
        public override string ToString() { return Data.ToString(); }
    }
}
