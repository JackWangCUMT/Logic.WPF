using System;

namespace Logic.Core
{
    public interface IStringSerializer
    {
        T Deserialize<T>(string str) where T : class;
        string Serialize<T>(T obj) where T : class;
    }
}
