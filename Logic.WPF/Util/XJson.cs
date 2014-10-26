using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Logic.WPF.Util
{
    public class XJson
    {
        #region Json

        public string JsonSerialize<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Objects,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
            return json;
        }

        public T JsonDeserialize<T>(string json)
        {
            var page = JsonConvert.DeserializeObject<T>(
                json,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
            return page;
        }

        #endregion

        #region Bson

        public byte[] BsonSerialize<T>(T obj)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                using (var writer = new BsonWriter(ms))
                {
                    var serializer = new JsonSerializer()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    };
                    serializer.Serialize(writer, obj);
                }
                return ms.ToArray();
            }
        }

        public T BsonDeserialize<T>(byte[] bson)
        {
            using (var ms = new System.IO.MemoryStream(bson))
            {
                using (BsonReader reader = new BsonReader(ms))
                {
                    var serializer = new JsonSerializer()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    };
                    var page = serializer.Deserialize<T>(reader);
                    return page;
                }
            }
        }

        #endregion
    }
}
