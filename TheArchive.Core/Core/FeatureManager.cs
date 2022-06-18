using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Settings;
using TheArchive.Utilities;

namespace TheArchive.Core
{
    public class FeatureManager : InitSingletonBase<FeatureManager>
    {

        public HashSet<Feature> RegisteredFeatures { get; private set; } = new HashSet<Feature>();

        private EnabledFeatures _enabledFeatures { get; set; }

        private HashSet<FeatureInternal.Update> _updateMethods = new HashSet<FeatureInternal.Update>();
        private HashSet<FeatureInternal.LateUpdate> _lateUpdateMethods = new HashSet<FeatureInternal.LateUpdate>();

        private Stack<FeatureInternal.Update> _updateToRemove = new Stack<FeatureInternal.Update>();
        private Stack<FeatureInternal.LateUpdate> _lateUpdateToRemove = new Stack<FeatureInternal.LateUpdate>();

        public event Action<Feature> OnFeatureEnabled;
        public event Action<Feature> OnFeatureDisabled;

        internal FeatureManager()
        {
            Feature.BuildInfo = ArchiveMod.CurrentBuildInfo;
            _enabledFeatures = LocalFiles.LoadConfig<EnabledFeatures>();
        }

        public void OnApplicationQuit()
        {
            ArchiveLogger.Info($"[{nameof(FeatureManager)}] {nameof(OnApplicationQuit)}()");
            try
            {
                ArchiveLogger.Info($"[{nameof(FeatureManager)}] Saving settings ... ");

                SaveConfig();

                foreach (var feature in RegisteredFeatures)
                {
                    SaveFeatureConfig(feature);
                }

                ArchiveLogger.Info($"[{nameof(FeatureManager)}] Unpatching ... ");

                foreach (var feature in RegisteredFeatures)
                {
                    feature.FeatureInternal.Quit();
                    
                    DisableFeature(feature);
                }
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"[{nameof(FeatureManager)}] Exception thrown in {nameof(OnApplicationQuit)}! {ex}: {ex.Message}");
                ArchiveLogger.Exception(ex);
            }
        }

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
            FeatureInternal.CreateAndAssign(feature);
            if (feature.Enabled)
            {
                if (feature.FeatureInternal.HasUpdateMethod)
                    _updateMethods.Add(feature.FeatureInternal.UpdateDelegate);
                if (feature.FeatureInternal.HasLateUpdateMethod)
                    _lateUpdateMethods.Add(feature.FeatureInternal.LateUpdateDelegate);
            }
            RegisteredFeatures.Add(feature);
        }

        public void EnableFeature(Feature feature, bool setConfig = true)
        {
            if (setConfig)
            {
                SetEnabledInConfig(feature, true);
            }

            if (feature.RequiresRestart) return;

            if (feature.Enabled) return;

            if (feature.AppliesToThisGameBuild)
                ArchiveLogger.Msg(ConsoleColor.Green, $"[{nameof(FeatureManager)}] Enabling {nameof(Feature)} {feature.Identifier} ...");

            feature.FeatureInternal.Enable();
            if (feature.FeatureInternal.HasUpdateMethod)
                _updateMethods.Add(feature.FeatureInternal.UpdateDelegate);
            if (feature.FeatureInternal.HasLateUpdateMethod)
                _lateUpdateMethods.Add(feature.FeatureInternal.LateUpdateDelegate);
            OnFeatureEnabled?.Invoke(feature);
        }

        public void DisableFeature(Feature feature, bool setConfig = true)
        {
            if (setConfig)
            {
                SetEnabledInConfig(feature, false);
            }

            if (feature.RequiresRestart) return;

            if (!feature.Enabled) return;

            if(feature.AppliesToThisGameBuild)
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

        public static void SaveFeatureConfig(Feature feature)
        {
            feature.FeatureInternal.SaveFeatureSettings();
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

        public static void ToggleFeature(Feature feature)
        {
            Instance.ToggleFeatureInstance(feature);
        }

        public void ToggleFeatureInstance(Feature feature)
        {
            bool enabled = (feature.AppliesToThisGameBuild && !feature.RequiresRestart) ? feature.Enabled : IsEnabledInConfig(feature);
            if (enabled)
            {
                DisableFeature(feature);
            }
            else
            {
                EnableFeature(feature);
            }
        }

        public void DEBUG_ENABLE()
        {
            foreach (var f in RegisteredFeatures)
            {
                EnableFeature(f);
            }
        }

        public void SaveConfig()
        {
            LocalFiles.SaveConfig(_enabledFeatures);
        }

        internal static void SetEnabledInConfig(Feature feature, bool value)
        {
            Instance.SetFeatureEnabledInConfig(feature, value);
        }

        private void SetFeatureEnabledInConfig(Feature feature, bool value)
        {
            if(_enabledFeatures.Features.TryGetValue(feature.Identifier, out var currentValue))
            {
                if(currentValue == value)
                {
                    return;
                }
                else
                {
                    _enabledFeatures.Features.Remove(feature.Identifier);
                }
            }
            _enabledFeatures.Features.Add(feature.Identifier, value);
        }

        public static bool IsEnabledInConfig(Feature feature)
        {
            return Instance.IsFeatureEnabledInConfig(feature);
        }

        private bool IsFeatureEnabledInConfig(Feature feature)
        {
            if(_enabledFeatures.Features.TryGetValue(feature.Identifier, out var value))
            {
                return value;
            }

            var shouldEnable = feature.GetType().GetCustomAttribute<Attributes.EnableFeatureByDefault>() != null;

            SetFeatureEnabledInConfig(feature, shouldEnable);
            return shouldEnable;
        }
    }
}
