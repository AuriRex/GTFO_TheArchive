#if false
using System;

namespace TheArchive.Core.Localization;
public class LocalizedTextJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        LocalizedText localizedText = (LocalizedText)value;
        if (localizedText.HasTranslation)
        {
            writer.WriteValue(localizedText.Id);
            return;
        }
        writer.WriteValue(localizedText.UntranslatedText);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        object value = reader.Value;
        if (value is string)
        {
            return new LocalizedText((string)reader.Value);
        }
        if (value is long)
        {
            return new LocalizedText((uint)(long)reader.Value);
        }
        return default(LocalizedText);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(LocalizedText);
    }
}
#endif