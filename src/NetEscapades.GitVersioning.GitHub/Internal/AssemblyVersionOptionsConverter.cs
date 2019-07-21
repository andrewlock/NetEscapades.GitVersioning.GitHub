// Adapted from https://github.com/AArnott/Nerdbank.GitVersioning/blob/9880338a60/src/NerdBank.GitVersioning/AssemblyVersionOptionsConverter.cs
using System;
using System.Reflection;
using Newtonsoft.Json;

namespace NetEscapades.GitVersioning.GitHub
{
    internal class AssemblyVersionOptionsConverter : JsonConverter
    {
        private readonly bool includeDefaults;

        internal AssemblyVersionOptionsConverter(bool includeDefaults)
        {
            this.includeDefaults = includeDefaults;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(VersionOptions.AssemblyVersionOptions).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType.Equals(typeof(VersionOptions.AssemblyVersionOptions)))
            {
                if (reader.Value is string)
                {
                    Version value;
                    if (Version.TryParse((string) reader.Value, out value))
                    {
                        return new VersionOptions.AssemblyVersionOptions(value);
                    }
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    // Temporarily remove ourselves from the serializer so we don't recurse infinitely.
                    serializer.Converters.Remove(this);
                    var result = serializer.Deserialize<VersionOptions.AssemblyVersionOptions>(reader);
                    serializer.Converters.Add(this);
                    return result;
                }
            }

            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var data = value as VersionOptions.AssemblyVersionOptions;
            if (data != null)
            {
                if (data.PrecisionOrDefault == VersionOptions.DefaultVersionPrecision && !this.includeDefaults)
                {
                    serializer.Serialize(writer, data.Version);
                    return;
                }
                else
                {
                    // Temporarily remove ourselves from the serializer so we don't recurse infinitely.
                    serializer.Converters.Remove(this);
                    serializer.Serialize(writer, data);
                    serializer.Converters.Add(this);
                    return;
                }
            }

            throw new NotSupportedException();
        }
    }
}