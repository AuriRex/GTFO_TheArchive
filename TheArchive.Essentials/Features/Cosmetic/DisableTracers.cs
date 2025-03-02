using FX_EffectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Cosmetic;

public class DisableTracers : Feature
{
    public override string Name => "Disable Bullet Tracers";

    public override FeatureGroup Group => FeatureGroups.Cosmetic;

    public override string Description => "Removes Bullet Tracer Effects";

    //protected FX_Trigger m_trigger;
    private static IValueAccessor<FX_EffectBase, FX_Trigger> _A_FX_EffectBase_m_trigger;

    public override void Init()
    {
        _A_FX_EffectBase_m_trigger = AccessorBase.GetValueAccessor<FX_EffectBase, FX_Trigger>("m_trigger");
    }


    [ArchivePatch(typeof(FX_TracerInstant), "SetPlayStart")]
    internal class FX_TracerInstant_Update_Patch
    {
        public static bool Prefix(FX_TracerInstant __instance)
        {
            _A_FX_EffectBase_m_trigger.Get(__instance).OnTriggeredDone();
            __instance.ReturnToPool();
            return ArchivePatch.SKIP_OG;
        }
    }

    [ArchivePatch(typeof(FX_TracerMoving), "SetPlayStart")]
    internal class FX_TracerMoving_Update_Patch
    {
        public static bool Prefix(FX_TracerMoving __instance)
        {
            _A_FX_EffectBase_m_trigger.Get(__instance).OnTriggeredDone();
            __instance.ReturnToPool();
            return ArchivePatch.SKIP_OG;
        }
    }
}