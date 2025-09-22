using Gear;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
public class FlickShotFix : Feature
{
    public override string Name => "Flick Shot Fix";

    public override string Description => "Before each single shot of the firearm, update the shooting direction to ensure it aligns with the camera's orientation, avoiding discrepancies between the actual firing direction and the crosshair.";

    public override GroupBase Group => GroupManager.Fixes;

    public const int PatchPriority = 10000;

    [ArchivePatch(typeof(BulletWeapon), nameof(BulletWeapon.Fire), priority: PatchPriority)]
    private static class BulletWeapon__Fire__Patch
    {
        private static void Prefix(BulletWeapon __instance)
        {
            if (__instance.Owner.IsLocallyOwned)
            {
                __instance.Owner.FPSCamera.UpdateCameraRay();
            }
        }
    }

    [ArchivePatch(typeof(Shotgun), nameof(Shotgun.Fire), priority: PatchPriority)]
    private static class Shotgun__Fire__Patch
    {
        private static void Prefix(Shotgun __instance)
        {
            if (__instance.Owner.IsLocallyOwned)
            {
                __instance.Owner.FPSCamera.UpdateCameraRay();
            }
        }
    }
}
