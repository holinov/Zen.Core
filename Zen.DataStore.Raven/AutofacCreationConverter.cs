using System;
using Autofac;
using Raven.Imports.Newtonsoft.Json;

namespace Zen.DataStore.Raven
{
    /// <summary>
    ///     Использование Autofac для десериализации из RavenDB
    /// </summary>
    public class AutofacCreationConverter : JsonConverter
    {
        /// <summary>
        ///     Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite
        {
            get { return false; }
        }

        public AutofacCreationConverter(AppCore scope)
        {
            Container = (AppScope)scope;
        }

        protected AppScope Container { get; set; }

        //public static ILifetimeScope Container { get; set; }

        /// <summary>
        ///     Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">
        ///     The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.
        /// </param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("AutofacCreationConverter should only be used while deserializing.");
        }

        /// <summary>
        ///     Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">
        ///     The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.
        /// </param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        ///     The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            using (var scope=Container.BeginScope())
            {
                object obj = scope.Resolve(objectType);
                if (obj == null)
                    throw new JsonSerializationException("No object created.");
                serializer.Populate(reader, obj);
                /*RefrenceHacks.SkipRefrences = true;
                scope.Scope.InjectUnsetProperties(obj);
                RefrenceHacks.SkipRefrences = false;*/
                return obj;
            }
        }


        /// <summary>
        ///     Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            using (var scope = Container.BeginScope())
            {
                return Container != null && scope.Scope.IsRegistered(objectType);
            }
        }
    }
}