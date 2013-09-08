using System;
using System.Collections;
using System.Collections.Generic;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Zen.DataStore.Raven
{
    public class RecordClrTypeInJsonContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (IsValidType(objectType))
                return new JsonObjectContract(objectType)
                    {
                        Converter = new RecordClrTypeInJsonConverter()
                    };
            return base.CreateContract(objectType);
        }

        private static bool IsValidType(Type objectType)
        {
            return objectType.IsGenericType &&
                   typeof (List<>) == (objectType.GetGenericTypeDefinition()); /* &&
                    objectType.GetGenericArguments()[0] == typeof(Event);*/
        }

        public class RecordClrTypeInJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteStartArray();
                foreach (var item in (IEnumerable) value)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("CrlType");
                    writer.WriteValue(item.GetType().AssemblyQualifiedName);


                    writer.WritePropertyName("Value");

                    serializer.Serialize(writer, item);

                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                            JsonSerializer serializer)
            {
                var list = (IList) Activator.CreateInstance(objectType);

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray)
                        break;


                    reader.Read(); //CrlType prop name
                    reader.Read(); //actual type

                    Type type = Type.GetType((string) reader.Value);

                    reader.Read(); // value property
                    reader.Read(); // actual value
                    object item = serializer.Deserialize(reader, type);
                    list.Add(item);
                    reader.Read(); // end object
                }
                return list;
            }

            public override bool CanConvert(Type objectType)
            {
                return IsValidType(objectType);
            }
        }
    }
}