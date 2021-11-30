using System;
using Newtonsoft.Json.Linq;
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
			Color color = (Color) value;
			writer.WriteStartObject();
			writer.WritePropertyName("a");
			writer.WriteValue(color.a);
			writer.WritePropertyName("r");
			writer.WriteValue(color.r);
			writer.WritePropertyName("g");
			writer.WriteValue(color.g);
			writer.WritePropertyName("b");
			writer.WriteValue(color.b);
			writer.WriteEndObject();
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Color) || objectType == typeof(Color32);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return default(Color);
			}
			JObject jobject = JObject.Load(reader);
			if (objectType == typeof(Color32))
			{
				return new Color32((byte) jobject["r"], (byte) jobject["g"], (byte) jobject["b"], (byte) jobject["a"]);
			}
			return new Color((float) jobject["r"], (float) jobject["g"], (float) jobject["b"], (float) jobject["a"]);
		}

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}
	}
}
