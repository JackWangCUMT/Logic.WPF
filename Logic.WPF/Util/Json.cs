using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Util
{
    public class Json
    {
        #region Json

        public string JsonSerialize<T>(T obj) where T : class
        {
            try
            {
                var json = JsonConvert.SerializeObject(
                    obj,
                    new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                    });
                return json;
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        public T JsonDeserialize<T>(string json) where T : class
        {
            try
            {
                var page = JsonConvert.DeserializeObject<T>(
                    json,
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                    });
                return page;
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        #endregion

        #region Bson

        public byte[] BsonSerialize<T>(T obj) where T : class
        {
            try
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var writer = new BsonWriter(ms))
                    {
                        var serializer = new JsonSerializer()
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                        };
                        serializer.Serialize(writer, obj);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        public T BsonDeserialize<T>(byte[] bson) where T : class
        {
            try
            {
                using (var ms = new System.IO.MemoryStream(bson))
                {
                    using (BsonReader reader = new BsonReader(ms))
                    {
                        var serializer = new JsonSerializer()
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                        };
                        var page = serializer.Deserialize<T>(reader);
                        return page;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        #endregion
    }
}
