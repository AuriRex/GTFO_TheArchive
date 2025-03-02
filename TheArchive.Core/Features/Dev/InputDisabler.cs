using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Features.Dev;

[AutomatedFeature]
[DoNotSaveToConfig]
internal class InputDisabler : Feature
{
    public override string Name => nameof(InputDisabler);

    public override FeatureGroup Group => FeatureGroups.Dev;

    public override string Description => "Used to disable input programatically.";

    public static bool Skip(ref bool __result)
    {
        __result = false;
        return ArchivePatch.SKIP_OG;
    }

    //public static bool GetButtonKeyMouseGamepad(InputAction action, eFocusState focusStateFilter = eFocusState.None)
    [ArchivePatch(typeof(InputMapper), nameof(InputMapper.GetButtonKeyMouseGamepad))]
    internal static class InputMapper_GetButtonKeyMouseGamepad_Patch
    {
        public static bool Prefix(ref bool __result) => Skip(ref __result);
    }

    //public static bool GetButtonDownKeyMouseGamepad(InputAction action, eFocusState focusStateFilter = eFocusState.None)
    [ArchivePatch(typeof(InputMapper), nameof(InputMapper.GetButtonDownKeyMouseGamepad))]
    internal static class InputMapper_GetButtonDownKeyMouseGamepad_Patch
    {
        public static bool Prefix(ref bool __result) => Skip(ref __result);
    }

    //public static bool GetButtonUpKeyMouseGamepad(InputAction action, eFocusState focusStateFilter = eFocusState.None)
    [ArchivePatch(typeof(InputMapper), nameof(InputMapper.GetButtonUpKeyMouseGamepad))]
    internal static class InputMapper_GetButtonUpKeyMouseGamepad_Patch
    {
        public static bool Prefix(ref bool __result) => Skip(ref __result);
    }

    //public static float GetAxisKeyMouseGamepad(InputAction action, eFocusState focusStateFilter = eFocusState.None)
    [ArchivePatch(typeof(InputMapper), nameof(InputMapper.GetAxisKeyMouseGamepad))]
    internal static class InputMapper_GetAxisKeyMouseGamepad_Patch
    {
        public static bool Prefix(ref float __result)
        {
            __result = 0f;
            return ArchivePatch.SKIP_OG;
        }
    }
}