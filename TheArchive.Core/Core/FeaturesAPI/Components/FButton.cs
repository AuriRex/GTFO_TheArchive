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

        /// <summary>
        /// Creates a button
        /// </summary>
        /// <param name="buttonText">The button text</param>
        /// <param name="buttonId">The buttons ID, default is the property name</param>
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
                JObject.Load(reader);
                return new FButton();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanWrite => false;
        }
    }
}
