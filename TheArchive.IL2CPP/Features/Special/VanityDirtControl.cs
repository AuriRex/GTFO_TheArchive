using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Special
{
    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    public class VanityDirtControl : Feature
    {
        public override string Name => "Vanity Dirt Control";

        public override string Group => FeatureGroups.Special;

        public override string Description => "Set all vanity items (clothes) dirt amount.";

        public override bool SkipInitialOnEnable => true;

        [FeatureConfig]
        public static VanityDirtControlSettings Settings { get; set; }

        public class VanityDirtControlSettings
        {
            [FSDisplayName("Dirt Amount")]
            [FSDescription("The amount of dirt on the player characters clothes.\n<b>0 to 1 range.</b>\n\n0 = clean\n1 = covered")]
            [FSSlider(0, 1)]
            public float DirtAmount { get; set; } = 0.75f;
        }

#if IL2CPP
        private static int _shaderPropertyID = -1;

        public static readonly string dirtAmountPropertyName = "_DirtAmount";

        public override void Init()
        {
            _shaderPropertyID = Shader.PropertyToID(dirtAmountPropertyName);
        }

        public override void OnEnable()
        {
            UpdateAllAgentsMaterials();
        }

        public override void OnDisable()
        {
            UpdateAllAgentsMaterials(0.75f);
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            UpdateAllAgentsMaterials();
        }

        private static void UpdateAllAgentsMaterials()
        {
            UpdateAllAgentsMaterials(Settings.DirtAmount);
        }

        private static void UpdateAllAgentsMaterials(float dirtValue)
        {
            if (SNetwork.SNet.Slots == null)
                return;

            var players = SNetwork.SNet.Slots.PlayersSynchedWithGame;

            if (players == null)
                return;

            foreach (var SNet_agent in players)
            {
                if (SNet_agent == null || !SNet_agent.HasPlayerAgent)
                    continue;

                var agent = SNet_agent.PlayerAgent.Cast<Player.PlayerAgent>();

                if (agent == null || agent.PlayerSyncModel == null)
                    continue;

                SetMaterialProperties(agent.PlayerSyncModel.m_gfxArms, dirtValue);
                SetMaterialProperties(agent.PlayerSyncModel.m_gfxHead, dirtValue);
                SetMaterialProperties(agent.PlayerSyncModel.m_gfxTorso, dirtValue);
                SetMaterialProperties(agent.PlayerSyncModel.m_gfxLegs, dirtValue);

                if(agent.IsLocallyOwned)
                {
                    SetMaterialProperties(agent.FPItemHolder?.FPSArms?.m_gfxArms, dirtValue);
                }
            }
        }

        private static void SetMaterialProperties(GameObject[] gfx, float dirtValue)
        {
            if (gfx == null)
                return;

            foreach (var go in gfx)
            {
                if (go == null)
                    continue;

                var meshRenderer = go.GetComponent<SkinnedMeshRenderer>();

                if (meshRenderer == null)
                    continue;

                foreach(var mat in meshRenderer.sharedMaterials)
                {
                    if (mat == null)
                        continue;

                    if (mat.HasProperty(_shaderPropertyID))
                    {
                        mat.SetFloat(_shaderPropertyID, dirtValue);
                    }
                }
            }
        }

        [ArchivePatch(nameof(PlayerSyncModelData.FindGfxParts))]
        internal static class PlayerSyncModelData_FindGfxParts_Patch
        {
            public static Type Type() => typeof(PlayerSyncModelData);

            public static void Postfix(PlayerSyncModelData __instance)
            {
                var dirtValue = Settings.DirtAmount;

                SetMaterialProperties(__instance.m_gfxArms, dirtValue);
                SetMaterialProperties(__instance.m_gfxHead, dirtValue);
                SetMaterialProperties(__instance.m_gfxTorso, dirtValue);
                SetMaterialProperties(__instance.m_gfxLegs, dirtValue);
            }
        }

        [ArchivePatch(nameof(PlayerFPSBody.UpdateModel))]
        internal static class PlayerFPSBody_UpdateModel_Patch
        {
            public static Type Type() => typeof(PlayerFPSBody);

            public static void Postfix(PlayerFPSBody __instance)
            {
                SetMaterialProperties(__instance.m_gfxArms, Settings.DirtAmount);
            }
        }
#endif
    }
}
