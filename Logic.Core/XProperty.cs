using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XProperty
    {
        public XProperty() { }
        public XProperty(object data)
            : this()
        {
            this.Data = data;
        }
        public object Data { get; set; }
    } 
}
