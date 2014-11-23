using Logic.Core;
using Logic.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Serialization
{
    public class Json : IStringSerializer
    {
        public string Serialize<T>(T obj) where T : class
        {
            try
            {
                var settings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Objects,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };
                settings.Converters.Add(new KeyValuePairConverter());
                var json = JsonConvert.SerializeObject(obj, settings);
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

        public T Deserialize<T>(string json) where T : class
        {
            try
            {
                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };
                settings.Converters.Add(new KeyValuePairConverter());
                var page = JsonConvert.DeserializeObject<T>(json, settings);
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
    }
}
