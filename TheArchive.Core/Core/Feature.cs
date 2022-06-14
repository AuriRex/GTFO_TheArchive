using TheArchive.Core.Models;

namespace TheArchive.Core
{
    /// <summary>
    /// An enableable / disableable feature
    /// </summary>
    public abstract class Feature
    {
        private string _identifier = null;
        public string Identifier => _identifier ??= this.GetType().Name;

        /// <summary>
        /// If the <see cref="Feature"/> is currently enabled.
        /// </summary>
        public bool Enabled { get; internal set; } = false;

        public bool AppliesToThisGameBuild => !FeatureInternal.InternalDisabled;

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
    }
}
