using System;
using Newtonsoft.Json;

namespace Telemachus
{
    /// <summary>
    /// Serializes Infinity and NaN floating-point values as JSON strings
    /// instead of producing invalid JSON tokens.
    /// </summary>
    class InfinityJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double) || objectType == typeof(float);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is double d && (double.IsInfinity(d) || double.IsNaN(d)))
            {
                writer.WriteValue(d.ToString());
            }
            else if (value is float f && (float.IsInfinity(f) || float.IsNaN(f)))
            {
                writer.WriteValue(f.ToString());
            }
            else
            {
                writer.WriteValue(value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }
}
