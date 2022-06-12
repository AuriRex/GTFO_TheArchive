using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Core
{
    public class FeatureManager : InitSingletonBase<FeatureManager>
    {

        public HashSet<Feature> RegisteredFeatures = new HashSet<Feature>();

        public event Action<Feature> OnFeatureEnabled;
        public event Action<Feature> OnFeatureDisabled;

        public void Init()
        {
            
        }

        public void InitFeature(Type type)
        {
            Feature feature = (Feature) Activator.CreateInstance(type);
            InitFeature(feature);
            RegisteredFeatures.Add(feature);
        }

        public void InitFeature(Feature feature)
        {
            feature.BuildInfo = ArchiveMod.CurrentBuildInfo;
            FeatureInternal.CreateAndAssign(feature);
            feature.Init();
        }

        public void EnableFeature(Feature feature)
        {
            ArchiveLogger.Msg(ConsoleColor.Green, $"[{nameof(FeatureManager)}] Enabling {nameof(Feature)} {feature.Identifier} ...");
            feature.FeatureInternal.Enable();
            OnFeatureEnabled?.Invoke(feature);
        }

        public void DisableFeature(Feature feature)
        {
            ArchiveLogger.Msg(ConsoleColor.Red, $"[{nameof(FeatureManager)}] Disabling {nameof(Feature)} {feature.Identifier} ...");
            feature.FeatureInternal.Disable();
            OnFeatureDisabled?.Invoke(feature);
        }

        internal static Feature GetById(string featureIdentifier)
        {
            return Instance.RegisteredFeatures.FirstOrDefault(f => f.Identifier == featureIdentifier);
        }

        internal static void Internal_Init()
        {
            Instance = new FeatureManager();
        }

        public void DEBUG_DISABLE()
        {
            foreach(var f in RegisteredFeatures)
            {
                DisableFeature(f);
            }
        }

        public void DEBUG_ENABLE()
        {
            foreach (var f in RegisteredFeatures)
            {
                EnableFeature(f);
            }
        }
    }
}
