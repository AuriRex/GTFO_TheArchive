using System;
using UnityEngine;

namespace TheArchive.Core.FeaturesAPI.Components
{
    /// <summary>
    /// Used to define a Mod Settings Button element.<br/>
    /// Make sure to <b>not</b> implement a setter on your property!
    /// </summary>
    public class FButton : ISettingsComponent
    {
        public string ButtonText { get; set; }
        internal string ButtonID { get; private set; }

        public bool HasPrimaryText => PrimaryText != null;

        public MonoBehaviour PrimaryText { get; set; }

        public bool HasSecondaryText => SecondaryText != null;

        public MonoBehaviour SecondaryText { get; set; }

        public bool HasCallback => Callback != null;

        internal Action Callback { get; set; } = null;

        public bool RefreshSubMenu { get; set; } = false;

        public FButton() { }

        /// <summary>
        /// Creates a button
        /// </summary>
        /// <param name="buttonText">The button text</param>
        /// <param name="buttonId">The buttons ID, default is the property name</param>
        /// <param name="callback">Action invoked on buttonpress.<br/>This fires even if the Feature is disabled!</param>
        /// <param name="refreshSubMenu">If the submenu the button is in should refresh after pressing.</param>
        public FButton(string buttonText, string buttonId = null, Action callback = null, bool refreshSubMenu = false)
        {
            ButtonText = buttonText;
            ButtonID = buttonId;
            Callback = callback;
            RefreshSubMenu = refreshSubMenu;
        }
    }
}
