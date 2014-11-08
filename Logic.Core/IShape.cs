using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Core
{
    public interface IShape
    {
        IStyle Style { get; set; }
        void Render(object dc, IRenderer renderer, IStyle style);
    }
}
