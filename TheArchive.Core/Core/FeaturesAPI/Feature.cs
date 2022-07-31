using System.Collections.Generic;
using TheArchive.Core.Models;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.FeaturesAPI
{
    /// <summary>
    /// An enableable / disableable feature
    /// </summary>
    public abstract class Feature
    {
        private string _identifier = null;
        public string Identifier => _identifier ??= this.GetType().Name;
        public bool IsHidden => FeatureInternal.HideInModSettings;
        public bool BelongsToGroup => Group != null;
        public bool HasAdditionalSettings => FeatureInternal.HasAdditionalSettings;
        public IEnumerable<FeatureSettingsHelper> SettingsHelpers => FeatureInternal.Settings;

        /// <summary>
        /// If the <see cref="Feature"/> is currently enabled.
        /// </summary>
        public bool Enabled { get; internal set; } = false;

        public bool AppliesToThisGameBuild => !FeatureInternal.InternalDisabled;

        public RundownFlags AppliesToRundowns => FeatureInternal.Rundowns;

        /// <summary>
        /// Information about the current game build.
        /// </summary>
        public static GameBuildInfo BuildInfo { get; internal set; }

        /// <summary>
        /// The Features Name,<br/> used in Mod Settings
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// A short description about this Feature,<br/> used in Mod Settings
        /// </summary>
        public virtual string Description { get; set; } = string.Empty;
        /// <summary>
        /// Used to group multiple settings together under one header,<br/>used in Mod Settings
        /// </summary>
        public virtual string Group => null;
        /// <summary>
        /// If set, prevents calling of <see cref="OnEnable"/> and <see cref="OnDisable"/> methods and only switches the config state of this <see cref="Feature"/>.
        /// </summary>
        public virtual bool RequiresRestart => false;


        /// <summary>
        /// Called once upon application start
        /// </summary>
        public virtual void Init()
        {
            
        }

        /// <summary>
        /// Called every time the feature gets enabled
        /// </summary>
        public virtual void OnEnable()
        {

        }

        /// <summary>
        /// Called every time the feature gets disabled
        /// </summary>
        public virtual void OnDisable()
        {

        }

        /// <summary>
        /// Called whenever the application quits
        /// </summary>
        public virtual void OnQuit()
        {

        }

        internal FeatureInternal FeatureInternal { get; set; }
        public static bool DevMode => ArchiveMod.Settings.FeatureDevMode;
    }
}
