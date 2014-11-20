using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Logic.WPF.Page
{
    public class NativeView : Canvas
    {
        public IRenderer Renderer { get; set; }
        public XContainer Container { get; set; }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (Renderer != null 
                && Container != null 
                && Container.Shapes != null)
            {
                foreach (var shape in Container.Shapes)
                {
                    shape.Render(dc, Renderer, shape.Style);
                }
            }
        }
    }
}
