using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IStringSerializer
    {
        T Deserialize<T>(string str) where T : class;
        string Serialize<T>(T obj) where T : class;
    }
}
