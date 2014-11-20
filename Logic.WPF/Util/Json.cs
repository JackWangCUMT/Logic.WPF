using Newtonsoft.Json;
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
        public string Serialize<T>(T obj) where T : class
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

        public T Deserialize<T>(string json) where T : class
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
    }
}
