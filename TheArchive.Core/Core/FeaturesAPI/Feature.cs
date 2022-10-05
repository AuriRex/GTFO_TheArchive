using System;
using System.Collections.Generic;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
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
        public void RequestRestart() => FeatureManager.RequestRestart(this);
        public void RevokeRestartRequest() => FeatureManager.RevokeRestartRequest(this);

        protected void RequestDisable(string reason = null) => FeatureInternal.RequestDisable(reason);

        /// <summary>
        /// True if this <see cref="Feature"/> is controled via code<br/>
        /// (button disabled in Mod Settings!)
        /// </summary>
        public bool IsAutomated => FeatureInternal.AutomatedFeature;

        /// <summary>
        /// Does what it says it does
        /// </summary>
        public bool DisableModSettingsButton => FeatureInternal.DisableModSettingsButton;

        /// <summary>
        /// Logging interface for this <see cref="Feature"/>
        /// </summary>
        public IArchiveLogger FeatureLogger => FeatureInternal.FeatureLoggerInstance;

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
        /// The <see cref="Feature"/>s Name<br/>
        /// used in Mod Settings
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// A text describing this <see cref="Feature"/><br/>
        /// used in Mod Settings
        /// </summary>
        public virtual string Description { get; set; } = string.Empty;

        /// <summary>
        /// Used to group multiple settings together under one header<br/>
        /// used in Mod Settings
        /// </summary>
        public virtual string Group => null;

        /// <summary>
        /// If set, prevents calling of <see cref="OnEnable"/> and <see cref="OnDisable"/> methods and only switches the config state of this <see cref="Feature"/>.
        /// </summary>
        public virtual bool RequiresRestart => false;

        /// <summary>
        /// If set, prevents calling of <see cref="OnEnable"/> on startup once.<br/>
        /// <see cref="OnEnable"/> gets called normally after that.
        /// </summary>
        public virtual bool SkipInitialOnEnable => false;

        /// <summary>
        /// If the <see cref="Feature"/> requires a UnityEngine AudioListener Component setup on the LocalPlayer GameObject
        /// </summary>
        public virtual bool RequiresUnityAudioListener => false;

        /// <summary>
        /// If this <see cref="Feature"/>s settings should be put into a sub menu inside of the Mod Settings menu
        /// </summary>
        public virtual bool PlaceSettingsInSubMenu => false;

        /// <summary>
        /// Called once upon application start before <see cref="Init"/> and before any patches have been loaded
        /// </summary>
        /// <returns>If the <see cref="Feature"/> should be inited</returns>
        public virtual bool PreInit()
        {
            return true;
        }

        /// <summary>
        /// Called once upon application start and after all patches have been loaded
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
        /// Called once after the game data has initialized
        /// </summary>
        public virtual void OnGameDataInitialized()
        {

        }

        /// <summary>
        /// Called once after datablocks have been loaded
        /// </summary>
        public virtual void OnDatablocksReady()
        {

        }

        /// <summary>
        /// Called everytime after a setting has been changed
        /// </summary>
        /// <param name="setting">The changed setting</param>
        public virtual void OnFeatureSettingChanged(FeatureSetting setting)
        {
            
        }

        /// <summary>
        /// Called everytime the gamestate changes<br/>
        /// Cast to <c>eGameStateName</c> or define a new instance method <c>OnGameStateChanged(eGameStateName state)</c>
        /// </summary>
        /// <param name="state">The state</param>
        public virtual void OnGameStateChanged(int state)
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
        public static bool GameDataInited { get; internal set; } = false;
        public static bool DataBlocksReady { get; internal set; } = false;
    }
}
