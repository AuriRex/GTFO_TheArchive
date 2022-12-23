using System;

namespace TheArchive.Core.FeaturesAPI.Components
{
    /// <summary>
    /// Used to define a Mod Settings Button element.<br/>
    /// Make sure to <b>not</b> implement a setter on your property!
    /// </summary>
    public class FButton
    {
        internal string ButtonText { get; private set; }
        internal string ButtonID { get; private set; }

        public FButton() { }

        public FButton(string buttonText, string buttonId = null)
        {
            ButtonText = buttonText;
            ButtonID = buttonId;
        }

        internal class FButtonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(FButton);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return new FButton();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                return;
            }
        }
    }
}
