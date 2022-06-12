using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Models;
using TheArchive.Interfaces;

namespace TheArchive.Core
{
    public abstract class Feature
    {
        private string _identifier = null;
        public string Identifier => _identifier ??= this.GetType().Name;

        /// <summary>
        /// If the <see cref="Feature"/> is currently enabled.
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Information about the current game build.
        /// </summary>
        public GameBuildInfo BuildInfo { get; internal set; }

        public abstract string Name { get; }
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

        internal FeatureInternal FeatureInternal { get; set; }
    }
}
