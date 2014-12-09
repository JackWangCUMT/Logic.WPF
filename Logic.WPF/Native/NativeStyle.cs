using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Logic.Native
{
    public class NativeStyle : IStyle
    {
        public string Name { get; set; }
        public IColor Fill { get; set; }
        public IColor Stroke { get; set; }
        public double Thickness { get; set; }

        public NativeStyle(string name, XColor fill, XColor stroke, double thickness)
        {
            Name = name;
            Fill = fill;
            Stroke = stroke;
            Thickness = thickness;
        }
    }
}
