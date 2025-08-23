/*using CellMenu;
using Globals;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace TheArchive.Features.LocalProgression;

[HideInModSettings]
[DoNotSaveToConfig]
[AutomatedFeature]
internal class ExpeditionUnlockSkip : Feature
{
    public override string Name => nameof(ExpeditionUnlockSkip);

    public override FeatureGroup Group => FeatureGroups.LocalProgression;

    public new static IArchiveLogger FeatureLogger { get; set; }

    public override void OnEnable()
    {
        if(BuildInfo.Rundown != Utilities.Utils.RundownID.RundownOne)
        {
            AllowFullRundown();
        }
    }

    public override void OnDisable()
    {
        if (BuildInfo.Rundown != Utilities.Utils.RundownID.RundownOne)
        {
            AllowFullRundown(false);
        }
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AllowFullRundown(bool allow = true)
    {
        FeatureLogger.Debug($"Global.AllowFullRundown set to {allow}.");
        Global.AllowFullRundown = allow;
    }

#if MONO
        //private void UpdateTierProgression(PlayerRundownProgression progression, List<CM_ExpeditionIcon_New> tierIcons, CM_RundownTierMarker tierMarker, bool tierUnlocked, out bool tierComplete)
        [RundownConstraint(Utilities.Utils.RundownFlags.RundownOne)]
        [ArchivePatch(typeof(CM_PageRundown_New), "UpdateTierProgression")]
        internal static class CM_PageRundown_New_UpdateTierProgression_Patch
        {
            public static void Prefix(PlayerRundownProgression progression, List<CM_ExpeditionIcon_New> tierIcons, CM_RundownTierMarker tierMarker, ref bool tierUnlocked)
            {
                tierUnlocked = true;
            }
        }
#endif
}*/