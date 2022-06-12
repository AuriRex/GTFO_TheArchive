using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Utilities;

namespace TheArchive.Core
{
    public class FeatureManager : InitSingletonBase<FeatureManager>
    {

        public HashSet<Feature> RegisteredFeatures { get; private set; } = new HashSet<Feature>();

        private HashSet<FeatureInternal.Update> _updateMethods = new HashSet<FeatureInternal.Update>();
        private HashSet<FeatureInternal.LateUpdate> _lateUpdateMethods = new HashSet<FeatureInternal.LateUpdate>();

        private Stack<FeatureInternal.Update> _updateToRemove = new Stack<FeatureInternal.Update>();
        private Stack<FeatureInternal.LateUpdate> _lateUpdateToRemove = new Stack<FeatureInternal.LateUpdate>();

        public event Action<Feature> OnFeatureEnabled;
        public event Action<Feature> OnFeatureDisabled;

        public void OnUpdate()
        {
            foreach(var update in _updateMethods)
            {
                try
                {
                    update.Invoke();
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Error($"[{nameof(FeatureManager)}] Update method on {update.Target.GetType().FullName} threw an exception! {ex}: {ex.Message}");
                    ArchiveLogger.Exception(ex);
                    _updateToRemove.Push(update);
                }
            }

            while (_updateToRemove.Count > 0)
            {
                _updateMethods.Remove(_updateToRemove.Pop());
            }
        }

        public void OnLateUpdate()
        {
            foreach (var lateUpdate in _lateUpdateMethods)
            {
                try
                {
                    lateUpdate.Invoke();
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Error($"[{nameof(FeatureManager)}] Update method on {lateUpdate.Target.GetType().FullName} threw an exception! {ex}: {ex.Message}");
                    ArchiveLogger.Exception(ex);
                    _lateUpdateToRemove.Push(lateUpdate);
                }
            }

            while (_lateUpdateToRemove.Count > 0)
            {
                _lateUpdateMethods.Remove(_lateUpdateToRemove.Pop());
            }
        }

        public void InitFeature(Type type)
        {
            Feature feature = (Feature) Activator.CreateInstance(type);
            InitFeature(feature);
        }

        private void InitFeature(Feature feature)
        {
            feature.BuildInfo = ArchiveMod.CurrentBuildInfo;
            FeatureInternal.CreateAndAssign(feature);
            if (feature.Enabled)
            {
                if (feature.FeatureInternal.HasUpdateMethod)
                    _updateMethods.Add(feature.FeatureInternal.UpdateDelegate);
                if (feature.FeatureInternal.HasLateUpdateMethod)
                    _lateUpdateMethods.Add(feature.FeatureInternal.LateUpdateDelegate);
            }
            feature.Init();
            RegisteredFeatures.Add(feature);
        }

        public void EnableFeature(Feature feature)
        {
            ArchiveLogger.Msg(ConsoleColor.Green, $"[{nameof(FeatureManager)}] Enabling {nameof(Feature)} {feature.Identifier} ...");
            feature.FeatureInternal.Enable();
            if (feature.FeatureInternal.HasUpdateMethod)
                _updateMethods.Add(feature.FeatureInternal.UpdateDelegate);
            if (feature.FeatureInternal.HasLateUpdateMethod)
                _lateUpdateMethods.Add(feature.FeatureInternal.LateUpdateDelegate);
            OnFeatureEnabled?.Invoke(feature);
        }

        public void DisableFeature(Feature feature)
        {
            ArchiveLogger.Msg(ConsoleColor.Red, $"[{nameof(FeatureManager)}] Disabling {nameof(Feature)} {feature.Identifier} ...");
            feature.FeatureInternal.Disable();
            if (feature.FeatureInternal.HasUpdateMethod)
                _updateMethods.Remove(feature.FeatureInternal.UpdateDelegate);
            if (feature.FeatureInternal.HasLateUpdateMethod)
                _lateUpdateMethods.Remove(feature.FeatureInternal.LateUpdateDelegate);
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
