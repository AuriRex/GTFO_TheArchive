using SNetwork;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Utilities;

namespace TheArchive.Features.Special
{
    [BuildConstraint(19715)]
    [RundownConstraint(Utils.RundownFlags.RundownOne)]
    public class R1SNetRevisionOverride : Feature
    {
        public override string Name => "R1 connect to 19087 games";

        public override FeatureGroup Group => FeatureGroups.Special;

        [IgnoreLocalization]
        public override string Description => Localization.Format(1, RevisionOverride, kOriginalRevision);

        public const int kOriginalRevision = 19715;
        public static int RevisionOverride { get; set; } = 19087;

        public override void OnEnable()
        {
            SNet.GameRevision = RevisionOverride;
            SNet.GameRevisionString = RevisionOverride.ToString();
        }

        public override void OnDisable()
        {
            SNet.GameRevision = kOriginalRevision;
            SNet.GameRevisionString = kOriginalRevision.ToString();
        }

        [ArchivePatch(typeof(SNet), nameof(SNet.Setup))]
        internal static class SNet_SetupPatch
        {
            public static void Prefix(ref int gameRevision)
            {
                var revisionOverride = 19087;
                ArchiveLogger.Notice($"SNET : Setting revision to {revisionOverride} (previous:{gameRevision})");
                gameRevision = revisionOverride;
            }
        }
    }
}
