using Logic.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF
{
    public class AppDependencies
    {
        public ICurrentApplication CurrentApplication { get; set; }
        public IFileDialog FileDialog { get; set; }
    }
}
