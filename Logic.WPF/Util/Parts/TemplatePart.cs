using Logic.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util.Parts
{
    public class TemplatePart
    {
        [ImportMany(typeof(ITemplate))]
        public IList<ITemplate> Templates { get; set; }
    }
}
