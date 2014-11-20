using System;

namespace Logic.Core
{
    public interface IBinarySerializer
    {
        T Deserialize<T>(byte[] data) where T : class;
        byte[] Serialize<T>(T obj) where T : class;
    }
}
