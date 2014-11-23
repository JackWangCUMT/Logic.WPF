using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IDocument
    {
        string Name { get; set; }
        IList<IPage> Pages { get; set; }
    }
}
