using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IStringSerializer
    {
        ILog Log { get; set; }
        T Deserialize<T>(string str) where T : class;
        string Serialize<T>(T obj) where T : class;
    }
}
