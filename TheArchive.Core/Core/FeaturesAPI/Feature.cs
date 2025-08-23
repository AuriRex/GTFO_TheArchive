using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.FeaturesAPI;

/// <summary>
/// A toggleable feature.
/// </summary>
/// <seealso cref="TheArchive.Core.Attributes.Feature.EnableFeatureByDefault"/>
/// <seealso cref="TheArchive.Core.Attributes.Feature.Patches.ArchivePatch"/>
/// <seealso cref="TheArchive.Core.Attributes.Feature.Members.FeatureConfig"/>
/// <example><code>
/// // Decorate your feature class with `TheArchive.Core.Attributes.Features` Attributes.
/// [EnableFeatureByDefault]
/// public class MyFeature : Feature
/// {
///     public override string Name => "My Cool Feature";
///     public override string Description => "Your Description here!\nNew lines are also ok!";
///  
///     // This gets set automatically so you can log things from within your patches!
///     public new static IArchiveLogger FeatureLogger { get; set; }
///  
///     [FeatureConfig] // Automatically saves and loads your settings.
///     public static MyCustomSettings Settings { get; set; }
///  
///     public class MyCustomSettings
///     {
///         // Use `TheArchive.Core.Attributes.Feature.Settings` Attributes for your settings.
///         [FSDisplayName("My Cool Setting")]
///         public string MySetting { get; set; } = "Default Value";
///     }
///   
///     // An archive patch class, essentially a harmony patch but attached to your feature.
///     // Unlike regular harmony patches, archive patches get toggled on/off with your feature.
///     [ArchivePatch(typeof(SomeTypeToPatch), nameof(SomeTypeToPatch.TheMethodToPatch))]
///     internal static class SomeTypeToPatch__TheMethodToPatch__Patch
///     {
///         public static void Postfix()
///         {
///             FeatureLogger.Notice("Postfix patch method is running!");
///         }
///     }
/// }
/// </code></example>
[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithInheritors)]
public abstract class Feature
{
    private string _identifier;
    /// <summary>
    /// This features unique identifier.
    /// </summary>
    public string Identifier => _identifier ??= GetType().Name;
    
    /// <summary>
    /// If this feature is hidden in the mod settings menu.
    /// </summary>
    public bool IsHidden => FeatureInternal.HideInModSettings;
    
    /// <summary>
    /// If this feature belongs to a group.
    /// </summary>
    public bool BelongsToGroup => Group != null;
    
    /// <summary>
    /// If the feature uses any properties decorated with <c>[FeatureConfig]</c>.
    /// </summary>
    public bool HasAdditionalSettings => FeatureInternal.HasAdditionalSettings;
    
    /// <summary>
    /// If all <c>[FeatureConfig]</c> settings are marked as hidden.
    /// </summary>
    public bool AllAdditionalSettingsAreHidden => FeatureInternal.AllAdditionalSettingsAreHidden;
    
    /// <summary>
    /// All used top-level settings helpers.
    /// </summary>
    public IEnumerable<FeatureSettingsHelper> SettingsHelpers => FeatureInternal.Settings;
    
    /// <summary>
    /// Request a game restart.
    /// </summary>
    /// <remarks>
    /// Adds a notice in the mod settings menu that some features are requesting a game restart.
    /// </remarks>
    public void RequestRestart() => FeatureManager.RequestRestart(this);
    
    /// <summary>
    /// Revoke your game restart request.
    /// </summary>
    public void RevokeRestartRequest() => FeatureManager.RevokeRestartRequest(this);

    /// <summary>
    /// Request this feature to be disabled.
    /// </summary>
    /// <param name="reason">The disable reason.</param>
    /// <remarks>
    /// <list>
    /// <item>This does not change the config state of your feature.</item>
    /// <item>The disabled reason is visible in game on the features description panel.</item>
    /// <item>Useful to add a reason in <see cref="ShouldInit"/>.</item>
    /// </list>
    /// </remarks>
    protected void RequestDisable(string reason = null) => FeatureInternal.RequestDisable(reason);

    /// <summary>
    /// This features localization service.
    /// </summary>
    public ILocalizationService Localization => FeatureInternal.Localization;

    /// <summary>
    /// Types that should be localized that aren't nested in your features type.
    /// </summary>
    public virtual Type[] LocalizationExternalTypes => Array.Empty<Type>();

    /// <summary>
    /// True if this feature is controlled via code.
    /// </summary>
    /// <remarks>
    /// Also disables the features button in the in-game mod settings button.
    /// </remarks>
    public bool IsAutomated => FeatureInternal.AutomatedFeature;

    /// <summary>
    /// Disables the button to toggles this feature in the in-game mod settings menu.
    /// </summary>
    public bool DisableModSettingsButton => FeatureInternal.DisableModSettingsButton;

    /// <summary>
    /// Logging interface for this feature.
    /// </summary>
    public IArchiveLogger FeatureLogger => FeatureInternal.FeatureLoggerInstance;

    /// <summary>
    /// If the feature is currently enabled.
    /// </summary>
    public bool Enabled { get; internal set; } = false;

    /// <summary>
    /// Feature is loaded and not disabled internally.
    /// </summary>
    public bool IsLoadedAndNotDisabledInternally => !FeatureInternal.InternalDisabled;

    /// <summary>
    /// The feature applies to those rundown game versions.
    /// </summary>
    public RundownFlags AppliesToRundowns => FeatureInternal.Rundowns;

    /// <summary>
    /// Information about the current game build.
    /// </summary>
    public static GameBuildInfo BuildInfo { get; internal set; }
    
    /// <summary>
    /// The features name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// A text describing this feature.
    /// </summary>
    /// <remarks>Can be multi-line using '\n'</remarks>
    public virtual string Description => string.Empty;

    /// <summary>
    /// The group to put your feature into.
    /// </summary>
    /// <remarks>
    /// By default, the module default feature group is used.
    /// </remarks>
    public virtual FeatureGroup Group => ModuleGroup;

    /// <summary>
    /// Default group for features that don't specify a custom one.
    /// </summary>
    protected FeatureGroup ModuleGroup => FeatureInternal.ModuleGroup;
    
    /// <summary>
    /// If set, prevents calling of <see cref="OnEnable"/> and <see cref="OnDisable"/> methods and instead only switches the config state of this feature.
    /// </summary>
    public virtual bool RequiresRestart => false;

    /// <summary>
    /// If set, prevents calling of <see cref="OnEnable"/> on startup once.<br/>
    /// <see cref="OnEnable"/> gets called normally after that.
    /// </summary>
    public virtual bool SkipInitialOnEnable => false;

    /// <summary>
    /// If the feature requires a UnityEngine AudioListener Component setup on the LocalPlayer GameObject.
    /// </summary>
    public virtual bool RequiresUnityAudioListener => false;

    /// <summary>
    /// If this features settings should be put inside its parent menu in the mod settings menu.
    /// </summary>
    public virtual bool InlineSettingsIntoParentMenu => false;

    /// <summary>
    /// Called once upon application start before <see cref="Init"/>, before any patches have been loaded.
    /// </summary>
    /// <returns>If the feature should be initialized.</returns>
    public virtual bool ShouldInit()
    {
        return true;
    }

    /// <summary>
    /// Called once upon application start before <see cref="Init"/>, after all patches and settings have been loaded.
    /// </summary>
    /// <returns>If the feature should be initialized.</returns>
    public virtual bool LateShouldInit()
    {
        return true;
    }

    /// <summary>
    /// Called once upon application start, after all patches and settings have been loaded.
    /// </summary>
    public virtual void Init()
    {
            
    }

    /// <summary>
    /// Called every time the feature gets enabled.
    /// </summary>
    /// <remarks>
    /// This won't be called the first time if <see cref="SkipInitialOnEnable"/> is set to true.
    /// </remarks>
    public virtual void OnEnable()
    {

    }

    /// <summary>
    /// Called every time the feature gets disabled.
    /// </summary>
    public virtual void OnDisable()
    {

    }

    /// <summary>
    /// Called once after the game data has been initialized.
    /// </summary>
    public virtual void OnGameDataInitialized()
    {

    }

    /// <summary>
    /// Called everytime the game is focused or unfocused.
    /// </summary>
    /// <param name="focus">The current application focus state of the game.</param>
    public virtual void OnApplicationFocusChanged(bool focus)
    {
            
    }

    /// <summary>
    /// Called once after datablocks have been loaded.
    /// </summary>
    /// <remarks>
    /// It is safe to call/use any game localization methods.
    /// </remarks>
    public virtual void OnDatablocksReady()
    {

    }

    /// <summary>
    /// Called once every frame whenever the feature is enabled.
    /// </summary>
    public virtual void Update()
    {

    }

    /// <summary>
    /// Called once every frame whenever the feature is enabled after all <see cref="Update"/>s have been called.
    /// </summary>
    public virtual void LateUpdate()
    {

    }

    /// <summary>
    /// Called everytime after a setting has been changed via the mod settings menu.
    /// </summary>
    /// <param name="setting">The changed setting.</param>
    public virtual void OnFeatureSettingChanged(FeatureSetting setting)
    {
            
    }

    /// <summary>
    /// Called everytime the game state changes.
    /// </summary>
    /// <param name="state">The new game state.</param>
    /// <remarks>
    /// Cast state value to <c>eGameStateName</c> or define a new instance method <c>OnGameStateChanged(eGameStateName state)</c>.
    /// </remarks>
    public virtual void OnGameStateChanged(int state)
    {

    }

    /// <summary>
    /// Called whenever an area is culled / unculled.
    /// </summary>
    /// <param name="lgArea">LG_Area that is affected</param>
    /// <param name="active">If rendered or not</param>
    /// <remarks>
    /// Cast to the first parameter to <c>LG_Area</c> or define a new instance method <c>OnAreaCull(LG_Area area, bool active)</c>.
    /// </remarks>
    [Obsolete("Has not been implemented properly; does not work!")]
    public virtual void OnAreaCull(object lgArea, bool active)
    {

    }

    /// <summary>
    /// Called whenever a FButton from this features mod settings menu has been pressed.
    /// </summary>
    /// <param name="setting">The <c>ButtonSetting</c> corresponding to the <c>FButton</c> that was pressed.</param>
    /// <remarks>
    /// <list>
    /// <item><b>Is only called if the feature is enabled!</b></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="FButton"/>
    public virtual void OnButtonPressed(ButtonSetting setting)
    {

    }

    /// <summary>
    /// Called whenever the application quits.
    /// </summary>
    public virtual void OnQuit()
    {

    }

    /// <summary>
    /// Call this to mark settings as dirty.<br/>
    /// Use for dictionary or other list type settings whenever changed through code!
    /// </summary>
    /// <param name="settings">The instance of the settings to mark dirty.</param>
    public bool MarkSettingsDirty(object settings) => FeatureInternal.MarkSettingsDirty(settings);

    /// <summary>
    /// Call this to mark settings as dirty.<br/>
    /// Use for dictionary or other list type settings whenever changed through code!
    /// </summary>
    /// <param name="settings">The instance of the settings object.</param>
    /// <param name="featureType">The Feature type the setting is implemented on.</param>
    /// <returns>Whether the setting was able to be set as dirty.</returns>
    /// <exception cref="InvalidOperationException">If no valid featureType was provided or found.</exception>
    /// <remarks>
    /// <list>
    /// <item>Only call from within a Feature class or provide the correct type!</item>
    /// <item>Not specifying <paramref name="featureType"/> will try to resolve it via a <see cref="StackTrace"/>!</item>
    /// </list>
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool MarkSettingsAsDirty(object settings, Type featureType = null)
    {
        if (featureType == null)
        {
            featureType = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            ArchiveLogger.Debug(featureType.FullName);
            int c = 0;
            while (!typeof(Feature).IsAssignableFrom(featureType) && c <= 5)
            {
                if (featureType.IsNested)
                {
                    featureType = featureType.DeclaringType;
                }
                c++;
            }
        }

        if (!typeof(Feature).IsAssignableFrom(featureType))
        {
            throw new InvalidOperationException("Only call from within a Feature class or provide the correct type!");
        }

        var feature = FeatureManager.GetByType(featureType) ?? throw new InvalidOperationException("Feature Type could not be found!");

        return feature.MarkSettingsDirty(settings);
    }

    /// <summary>
    /// Call this to mark settings as dirty.<br/>
    /// Use for dictionary or other list type settings whenever changed through code!<br/>
    /// <paramref name="settings"/> must be a <b>nested class</b> inside your feature implementation!<br/>
    /// Use <see cref="MarkSettingsAsDirty(object, Type)"/> instead if the above is not the case.
    /// </summary>
    /// <typeparam name="T">A <b>nested class</b> inside your feature implementation</typeparam>
    /// <param name="settings">The instance of the settings object.</param>
    /// <returns>Whether the setting was able to be set as dirty.</returns>
    /// <exception cref="InvalidOperationException">If no valid featureType was provided or found.</exception>
    /// <remarks>
    /// <list>
    /// <item>Type <typeparamref name="T"/> must be a <b>nested class</b> inside your feature implementation!</item>
    /// </list>
    /// </remarks>
    public static bool MarkSettingsAsDirty<T>(T settings)
    {
        if (!typeof(T).IsNested)
            throw new InvalidOperationException("Type must be a nested class of your Feature implementation to use this method!");

        var featureType = typeof(T).DeclaringType;

        return MarkSettingsAsDirty(settings, featureType);
    }

    internal FeatureInternal FeatureInternal { get; set; }
    
    /// <inheritdoc cref="ArchiveMod.IsPlayingModded"/>
    public static bool IsPlayingModded => ArchiveMod.IsPlayingModded;
    
    /// <summary>
    /// If dev mode is enabled.
    /// </summary>
    public static bool DevMode => ArchiveMod.Settings.FeatureDevMode;
    
    /// <summary>
    /// Has game data been initialized yet?
    /// </summary>
    public static bool GameDataInited { get; internal set; }
    
    /// <summary>
    /// Is the game currently in focus?
    /// </summary>
    public static bool IsApplicationFocused { get; internal set; }
    
    /// <summary>
    /// Is the game currently in the process of quitting?
    /// </summary>
    public static bool IsApplicationQuitting { get; internal set; }
    
    /// <summary>
    /// Have data blocks been initialized yet?
    /// </summary>
    public static bool DataBlocksReady { get; internal set; }
    
    /// <summary>
    /// Cast to <c>eGameStateName</c>.
    /// </summary>
    public static int CurrentGameState { get; internal set; }
    
    /// <summary>
    /// Cast to <c>eGameStateName</c>.
    /// </summary>
    public static int PreviousGameState { get; internal set; }

    internal static void SetupIs()
    {
        Is.R1 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownOne);
        Is.R1OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownOne.ToLatest());
        Is.R2 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownTwo);
        Is.R2OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownTwo.ToLatest());
        Is.R3 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownThree);
        Is.R3OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownThree.ToLatest());
        Is.R4 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFour);
        Is.R4OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFour.ToLatest());
        Is.R5 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFive);
        Is.R5OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFive.ToLatest());
        Is.R6 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix);
        Is.R6OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest());
        Is.R7 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSeven);
        Is.R7OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSeven.ToLatest());
        Is.A1 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltOne);
        Is.A1OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltOne.ToLatest());
        Is.A2 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltTwo);
        Is.A2OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltTwo.ToLatest());
        Is.A3 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltThree);
        Is.A3OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltThree.ToLatest());
        Is.A4 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltFour);
        Is.A4OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltFour.ToLatest());
        Is.A5 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltFive);
        Is.A5OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltFive.ToLatest());
        Is.A6 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltSix);
        Is.A6OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownAltSix.ToLatest());
        Is.R8 = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownEight);
        Is.R8OrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownEight.ToLatest());
    }

    /// <summary>
    /// If the currently running game version is [...]
    /// </summary>
    /// <remarks>
    /// Mostly unused now.
    /// </remarks>
    public static class Is
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool R1 { get; internal set; }
        public static bool R1OrLater { get; internal set; }
        public static bool R2 { get; internal set; }
        public static bool R2OrLater { get; internal set; }
        public static bool R3 { get; internal set; }
        public static bool R3OrLater { get; internal set; }
        public static bool R4 { get; internal set; }
        public static bool R4OrLater { get; internal set; }
        public static bool R5 { get; internal set; }
        public static bool R5OrLater { get; internal set; }
        public static bool R6 { get; internal set; }
        public static bool R6OrLater { get; internal set; }
        public static bool R7 { get; internal set; }
        public static bool R7OrLater { get; internal set; }
        public static bool A1 { get; internal set; }
        public static bool A1OrLater { get; internal set; }
        public static bool A2 { get; internal set; }
        public static bool A2OrLater { get; internal set; }
        public static bool A3 { get; internal set; }
        public static bool A3OrLater { get; internal set; }
        public static bool A4 { get; internal set; }
        public static bool A4OrLater { get; internal set; }
        public static bool A5 { get; internal set; }
        public static bool A5OrLater { get; internal set; }
        public static bool A6 { get; internal set; }
        public static bool A6OrLater { get; internal set; }
        public static bool R8 { get; internal set; }
        public static bool R8OrLater { get; internal set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}