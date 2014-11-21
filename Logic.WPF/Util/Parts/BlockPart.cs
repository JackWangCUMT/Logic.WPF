using Logic.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Util.Parts
{
    public class BlockPart
    {
        [ImportMany(typeof(XBlock))]
        public IList<XBlock> Blocks { get; set; }
    }
}
