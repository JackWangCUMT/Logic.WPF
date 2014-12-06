using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XStyle : IStyle
    {
        public string Name { get; set; }
        public IColor Fill { get; set; }
        public IColor Stroke { get; set; }
        public double Thickness { get; set; }
        public object NativeFill() { return null; }
        public object NativeStroke() { return null; }
    }
}
