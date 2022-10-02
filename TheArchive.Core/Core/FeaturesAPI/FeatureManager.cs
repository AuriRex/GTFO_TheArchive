using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Settings;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Core.FeaturesAPI
{
    public class FeatureManager : InitSingletonBase<FeatureManager>
    {

        public HashSet<Feature> RegisteredFeatures { get; private set; } = new HashSet<Feature>();
        public HashSet<Feature> FeaturesRequestingRestart { get; private set; } = new HashSet<Feature>();
        public bool AnyFeatureRequestingRestart => FeaturesRequestingRestart.Count > 0;

        public Dictionary<string, HashSet<Feature>> GroupedFeatures { get; private set; } = new Dictionary<string, HashSet<Feature>>();

        private EnabledFeatures _enabledFeatures { get; set; }


        private HashSet<FeatureInternal.Update> _updateMethods = new HashSet<FeatureInternal.Update>();
        private HashSet<FeatureInternal.LateUpdate> _lateUpdateMethods = new HashSet<FeatureInternal.LateUpdate>();

        private Stack<FeatureInternal.Update> _updateToRemove = new Stack<FeatureInternal.Update>();
        private Stack<FeatureInternal.LateUpdate> _lateUpdateToRemove = new Stack<FeatureInternal.LateUpdate>();

        private Feature _unityAudioListenerHelper;
        private Feature UnityAudioListenerHelper
        {
            get
            {
                if(_unityAudioListenerHelper == null)
                {
                    _unityAudioListenerHelper = GetById("InternalUAudioListenerHelper");
                }
                return _unityAudioListenerHelper;
            }
        }

        private IArchiveLogger _logger = LoaderWrapper.CreateArSubLoggerInstance(nameof(FeatureManager), ConsoleColor.DarkYellow);

        public event Action<Feature> OnFeatureEnabled;
        public event Action<Feature> OnFeatureDisabled;
        public event Action<Feature, bool> OnFeatureRestartRequestChanged;

        private FeatureManager()
        {
            Feature.BuildInfo = ArchiveMod.CurrentBuildInfo;
            _enabledFeatures = LocalFiles.LoadConfig<EnabledFeatures>();
            ArchiveMod.GameStateChanged += OnGameStateChanged;
        }

        internal void OnGameDataInitialized()
        {
            _logger.Debug($"{nameof(OnGameDataInitialized)}()");
            Feature.GameDataInited = true;
            try
            {
                foreach (var feature in RegisteredFeatures)
                {
                    feature.FeatureInternal.GameDataInitialized();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(OnGameDataInitialized)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }

        internal void OnDatablocksReady()
        {
            _logger.Debug($"{nameof(OnDatablocksReady)}()");
            Feature.DataBlocksReady = true;
            try
            {
                foreach (var feature in RegisteredFeatures)
                {
                    feature.FeatureInternal.DatablocksReady();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(OnDatablocksReady)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }

        internal void OnApplicationQuit()
        {
            _logger.Info($"{nameof(OnApplicationQuit)}()");
            try
            {
                _logger.Info($"Saving settings ... ");

                SaveConfig();

                foreach (var feature in RegisteredFeatures)
                {
                    SaveFeatureConfig(feature);
                }

                _logger.Info($"Unpatching ... ");

                foreach (var feature in RegisteredFeatures)
                {
                    feature.FeatureInternal.Quit();
                    
                    DisableFeature(feature);
                }
            }
            catch(Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(OnApplicationQuit)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }

        internal void OnUpdate()
        {
            foreach(var update in _updateMethods)
            {
                try
                {
                    update.Invoke();
                }
                catch(Exception ex)
                {
                    _logger.Error($"Update method on {update.Target.GetType().FullName} threw an exception! {ex}: {ex.Message}");
                    _logger.Exception(ex);
                    _logger.Warning($"Removing Update method on {update.Target.GetType().FullName}! (Update won't be called anymore!!)");
                    _updateToRemove.Push(update);
                }
            }

            while (_updateToRemove.Count > 0)
            {
                _updateMethods.Remove(_updateToRemove.Pop());
            }
        }

        internal void OnLateUpdate()
        {
            foreach (var lateUpdate in _lateUpdateMethods)
            {
                try
                {
                    lateUpdate.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.Error($"LateUpdate method on {lateUpdate.Target.GetType().FullName} threw an exception! {ex}: {ex.Message}");
                    _logger.Exception(ex);
                    _logger.Warning($"Removing LateUpdate method on {lateUpdate.Target.GetType().FullName}! (LateUpdate won't be called anymore!!)");
                    _lateUpdateToRemove.Push(lateUpdate);
                }
            }

            while (_lateUpdateToRemove.Count > 0)
            {
                _lateUpdateMethods.Remove(_lateUpdateToRemove.Pop());
            }
        }

        internal void OnFeatureSettingChanged(FeatureSetting setting)
        {
            setting.Helper.Feature.FeatureInternal.FeatureSettingChanged(setting);
        }

        private void OnGameStateChanged(int state)
        {
            foreach(var feature in RegisteredFeatures)
            {
                if (!feature.Enabled) continue;

                feature.FeatureInternal.GameStateChanged(state);
            }
        }

        public void InitFeature(Type type)
        {
            Feature feature = (Feature) Activator.CreateInstance(type);
            InitFeature(feature);
            CheckSpecialFeatures();
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
            if (feature == null) return;

            if (setConfig)
            {
                SetEnabledInConfig(feature, true);
            }

            if (feature.RequiresRestart) return;

            if (feature.Enabled) return;

            if (!feature.AppliesToThisGameBuild) return;

            _logger.Msg(ConsoleColor.Green, $"Enabling {(feature.IsAutomated ? "automated " : String.Empty)}{nameof(Feature)} {feature.Identifier} ...");

            feature.FeatureInternal.Enable();
            if (feature.FeatureInternal.HasUpdateMethod)
                _updateMethods.Add(feature.FeatureInternal.UpdateDelegate);
            if (feature.FeatureInternal.HasLateUpdateMethod)
                _lateUpdateMethods.Add(feature.FeatureInternal.LateUpdateDelegate);
            OnFeatureEnabled?.Invoke(feature);
        }

        public void DisableFeature(Feature feature, bool setConfig = true)
        {
            if (feature == null) return;

            if (setConfig)
            {
                SetEnabledInConfig(feature, false);
            }

            if (feature.RequiresRestart) return;

            if (!feature.Enabled) return;

            if(feature.AppliesToThisGameBuild)
                _logger.Msg(ConsoleColor.Red, $"Disabling {nameof(Feature)} {feature.Identifier} ...");

            feature.FeatureInternal.Disable();
            if (feature.FeatureInternal.HasUpdateMethod)
                _updateMethods.Remove(feature.FeatureInternal.UpdateDelegate);
            if (feature.FeatureInternal.HasLateUpdateMethod)
                _lateUpdateMethods.Remove(feature.FeatureInternal.LateUpdateDelegate);
            OnFeatureDisabled?.Invoke(feature);
        }

        public void CheckSpecialFeatures()
        {
            var requireAudioListner = RegisteredFeatures.Where(f => f.Enabled && f.RequiresUnityAudioListener);
            if (requireAudioListner.Count() > 0)
            {
                if (UnityAudioListenerHelper != null && !UnityAudioListenerHelper.Enabled)
                {
                    _logger.Notice($"Some Features require a UnityEngine AudioListener: [{string.Join("], [", requireAudioListner.Select(f => f.Identifier))}]");
                    EnableFeature(UnityAudioListenerHelper);
                }
            }
            else
            {
                if (UnityAudioListenerHelper != null && UnityAudioListenerHelper.Enabled)
                    DisableFeature(UnityAudioListenerHelper);
            }
        }

        public static void EnableAutomatedFeature(Type type)
        {
            var feature = Instance.RegisteredFeatures.FirstOrDefault(x => x.GetType() == type);

            if (feature == null) return;

            if (!feature.IsAutomated) return;

            Instance.EnableFeature(feature, false);
        }

        public static void DisableAutomatedFeature(Type type)
        {
            var feature = Instance.RegisteredFeatures.FirstOrDefault(x => x.GetType() == type);

            if (feature == null) return;

            if (!feature.IsAutomated) return;

            Instance.DisableFeature(feature, false);
        }

        internal static void RequestRestart(Feature feature)
        {
            Instance.FeaturesRequestingRestart.Add(feature);
            Instance.OnFeatureRestartRequestChanged?.Invoke(feature, true);
        }

        internal static void RevokeRestartRequest(Feature feature)
        {
            Instance.FeaturesRequestingRestart.Remove(feature);
            Instance.OnFeatureRestartRequestChanged?.Invoke(feature, false);
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

            if (feature.RequiresRestart && feature.FeatureInternal.InitialEnabledState == !enabled && !FeaturesRequestingRestart.Contains(feature))
            {
                feature.RequestRestart();
            }

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
            if (feature.FeatureInternal.DoNotSaveToConfig) return;

            if (_enabledFeatures.Features.TryGetValue(feature.Identifier, out var currentValue))
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
            if (feature.FeatureInternal.DoNotSaveToConfig)
            {
                if(feature.FeatureInternal.AutomatedFeature)
                    return false;
                return true;
            }

            if (_enabledFeatures.Features.TryGetValue(feature.Identifier, out var value))
            {
                return value;
            }

            var shouldEnable = feature.GetType().GetCustomAttribute<Attributes.EnableFeatureByDefault>() != null;

            SetFeatureEnabledInConfig(feature, shouldEnable);
            return shouldEnable;
        }

        internal static void AddGroupedFeature(Feature feature)
        {
            Instance.AddGroupedFeatureI(feature);
        }

        private void AddGroupedFeatureI(Feature feature)
        {
            if (feature == null || !feature.BelongsToGroup) return;

            if(!GroupedFeatures.TryGetValue(feature.Group, out var featureSet))
            {
                GroupedFeatures.Add(feature.Group, new HashSet<Feature>() {
                    feature
                });
                return;
            }

            featureSet.Add(feature);
        }
    }
}
