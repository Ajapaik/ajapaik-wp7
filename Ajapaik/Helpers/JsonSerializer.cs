﻿using System.IO;
using System.Runtime.Serialization.Json;

namespace Ajapaik.Helpers
{
    public class JsonSerializer
    {
        public static string Serialize<T>(T instance) where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, instance);

                memoryStream.Flush();
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static T Deserialize<T>(string serialized) where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                {
                    writer.Write(serialized);
                    writer.Flush();

                    memoryStream.Position = 0;

                    return serializer.ReadObject(memoryStream) as T;
                }
            }
        }
    }
}