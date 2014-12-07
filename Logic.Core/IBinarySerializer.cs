using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IBinarySerializer
    {
        ILog Log { get; set; }
        T Deserialize<T>(byte[] data) where T : class;
        byte[] Serialize<T>(T obj) where T : class;
    }
}
