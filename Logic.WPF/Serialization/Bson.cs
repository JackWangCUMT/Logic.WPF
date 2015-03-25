// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Serialization
{
    public class Bson : IBinarySerializer
    {
        public byte[] Serialize<T>(T obj) where T : class
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

        public T Deserialize<T>(byte[] bson) where T : class
        {
            using (var ms = new System.IO.MemoryStream(bson))
            {
                using (BsonReader reader = new BsonReader(ms))
                {
                    var serializer = new JsonSerializer()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                        ContractResolver = new LogicContractResolver()
                    };
                    var page = serializer.Deserialize<T>(reader);
                    return page;
                }
            }
        }
    }
}
