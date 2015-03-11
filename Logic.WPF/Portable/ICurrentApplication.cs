using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Portable
{
    public interface ICurrentApplication
    {
        void Close();
        void Invoke(Action callback);
    }
}
