using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.Accessibility;

public class DisableUIMirroring : Feature
{
    public override string Name => "Disable UI Mirroring";

    public override FeatureGroup Group => FeatureGroups.Accessibility;

    public override string Description => "Removes the mirroring effect of UI elements.";

    private static IValueAccessor<UI_Apply, float> A_s_mirrorOpacity;
    private static IValueAccessor<UI_Apply, float> A_s_reflectionOpacity;
    private static MethodAccessor<UI_Apply> A_ApplySetttings;

    public override void Init()
    {
        A_s_mirrorOpacity = AccessorBase.GetValueAccessor<UI_Apply, float>("s_mirrorOpacity", true);
        A_s_reflectionOpacity = AccessorBase.GetValueAccessor<UI_Apply, float>("s_reflectionOpacity", true);
        A_ApplySetttings = MethodAccessor<UI_Apply>.GetAccessor("ApplySetttings", Array.Empty<Type>());
    }

    public new static IArchiveLogger FeatureLogger { get; set; }

    public override void OnEnable()
    {
        A_ApplySetttings.Invoke(null);
    }

    public override void OnDisable()
    {
        A_s_mirrorOpacity.Set(null, 0.4f);
        A_s_reflectionOpacity.Set(null, 0.05f);

        A_ApplySetttings.Invoke(null);
    }

    // Yes, it has 3 ´t´s ¯\_(ツ)_/¯
    [ArchivePatch(typeof(UI_Apply), "ApplySetttings", new Type[0])]
    internal static class UI_Apply_ApplySetttings_Patch
    {
        public static void Prefix()
        {
            A_s_mirrorOpacity.Set(null, 0f);
            A_s_reflectionOpacity.Set(null, 0f);
        }
    }
}