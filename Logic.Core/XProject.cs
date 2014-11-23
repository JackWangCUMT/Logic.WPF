using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XProject : IProject
    {
        public string Name { get; set; }
        public IList<IStyle> Styles { get; set; }
        public IList<ITemplate> Templates { get; set; }
        public IList<IDocument> Documents { get; set; }
    }
}
