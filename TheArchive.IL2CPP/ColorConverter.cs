/* https://github.com/ianmacgillivray/Json-NET-for-Unity/blob/master/Source/Newtonsoft.Json/Converters/ColorConverter.cs
MIT License

Copyright (c) 2017 parentelement

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Newtonsoft.Json.Converters
{
    public class ColorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var col = (Color) value;
            writer.WriteStartObject();
            writer.WritePropertyName("a");
            writer.WriteValue(col.a);
            writer.WritePropertyName("r");
            writer.WriteValue(col.r);
            writer.WritePropertyName("g");
            writer.WriteValue(col.g);
            writer.WritePropertyName("b");
            writer.WriteValue(col.b);
            writer.WriteEndObject();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color) || objectType == typeof(Color32);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new Color();

            var obj = JObject.Load(reader);

            if (objectType == typeof(Color32))
                return new Color32((byte) obj["r"], (byte) obj["g"], (byte) obj["b"], (byte) obj["a"]);

            return new Color((float) obj["r"], (float) obj["g"], (float) obj["b"], (float) obj["a"]);
        }

        public override bool CanRead
        {
        get { return true; }
        }
    }
}