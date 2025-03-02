using CullingSystem;
using LevelGeneration;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.Dev;

//[EnableFeatureByDefault]
[HideInModSettings]
[DoNotSaveToConfig]
[ForceDisable]
#warning TODO: Rework "Special" Features system, include this one.
public class CullingHook : Feature
{
    public override string Name => nameof(CullingHook);

    public override FeatureGroup Group => FeatureGroups.Dev;

    public new static IArchiveLogger FeatureLogger { get; set; }

    public static void CallUpdate(IC_CullbucketOwner owner, bool active)
    {
        var lgArea = owner.TryCastTo<LG_Area>();

        if (lgArea == null)
            return;

        //FeatureLogger.Debug($"active={active} : Zone {lgArea.m_zone.Alias} Area {lgArea.m_navInfo.Suffix}");
        FeatureManager.Instance.OnLGAreaCullUpdate(lgArea, active);
    }

    [ArchivePatch(typeof(C_Node), nameof(C_Node.Show))]
    internal static class C_Node_Show_Patch
    {
        private static bool _IsShown;
        public static void Prefix(C_Node __instance)
        {
            _IsShown = __instance.IsShown;
        }

        public static void Postfix(C_Node __instance)
        {
            if (_IsShown)
            {
                return;
            }
            CallUpdate(__instance.m_owner, true);
        }
    }

    [ArchivePatch(typeof(C_Node), nameof(C_Node.Hide))]
    internal static class C_Node_Hide_Patch
    {
        private static bool _IsShown;
        public static void Prefix(C_Node __instance)
        {
            _IsShown = __instance.IsShown;
        }

        public static void Postfix(C_Node __instance)
        {
            if (!_IsShown)
            {
                return;
            }
            CallUpdate(__instance.m_owner, false);
        }
    }
}