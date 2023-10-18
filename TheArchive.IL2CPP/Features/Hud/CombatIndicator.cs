using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Features.Dev;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Hud
{
    public class CombatIndicator : Feature
    {
        public override string Name => "Combat Indicator";

        public override string Group => FeatureGroups.Hud;

        public override string Description => "Displays the current drama state of the game.\n(Above the health bar, right side)\n\nBasically a visual representation of what the music is doing.";

        public override bool SkipInitialOnEnable => true;

        public static new IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static CombatIndicatorSettings Settings { get; set; }

        public class CombatIndicatorSettings
        {
            public CombatIndicatorStyle Style { get; set; } = CombatIndicatorStyle.Simple;

            public enum CombatIndicatorStyle
            {
                Simple,
                Detailed
            }
        }

        /*
         * PUI_LocalPlayerStatus
         * HealthText
         * parent: one above HealthText's parent (GUI/CellUI_Camera(Clone)/PlayerLayer/MovementRoot/PUI_LocalPlayerStatus_CellUI(Clone)/)
         * xyz: 301 30 0
         * align: Right
         * fontSize: 15
         * characterSpacing: -4.5
         */

        private static DRAMA_State _DRAMA_State_Encounter;

        public override void Init()
        {
            _DRAMA_State_Encounter = Utils.GetEnumFromName<DRAMA_State>(nameof(DRAMA_State.Encounter));
        }

        public override void OnEnable()
        {
            if (GetOrSetupCombatIndicator(out var ci))
                CurrentCombatIndicator = ci;
        }

        public override void OnDisable()
        {
            if (IsApplicationQuitting)
                return;

            if (CurrentCombatIndicator != null)
                Object.Destroy(CurrentCombatIndicator.gameObject);
        }

        public static TMPro.TextMeshPro CurrentCombatIndicator { get; private set; }
        public static bool HasCombatIndicator => CurrentCombatIndicator != null;

        public static bool GetOrSetupCombatIndicator(out TMPro.TextMeshPro combatIndicatorTMP)
        {
            return GetOrSetupCombatIndicator(GuiManager.PlayerLayer?.m_playerStatus, out combatIndicatorTMP);
        }

        public static bool GetOrSetupCombatIndicator(PUI_LocalPlayerStatus localPlayerStatus, out TMPro.TextMeshPro combatIndicatorTMP)
        {
            combatIndicatorTMP = null;

            if (localPlayerStatus == null)
                return false;

            var existing = localPlayerStatus.transform.GetChildWithExactName(nameof(CombatIndicator));
            if (existing != null)
            {
                combatIndicatorTMP = existing.GetComponent<TMPro.TextMeshPro>();
                return true;
            }

            combatIndicatorTMP = Object.Instantiate(localPlayerStatus.m_healthText);

            combatIndicatorTMP.name = nameof(CombatIndicator);

            combatIndicatorTMP.transform.SetParent(localPlayerStatus.transform);

            combatIndicatorTMP.transform.localPosition = new Vector3(245f, 27.5f, 0);

            float scale = .625f;
            combatIndicatorTMP.transform.localScale = new Vector3(scale, scale, scale);

            combatIndicatorTMP.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 25);

            combatIndicatorTMP.alignment = TMPro.TextAlignmentOptions.Right;
            //combatIndicatorTMP.fontSize = 15;
            combatIndicatorTMP.characterSpacing = -4.5f;
            combatIndicatorTMP.text = GetStateString(DramaManager.CurrentStateEnum);

            combatIndicatorTMP.color = Color.white.WithAlpha(0.1961f);

            combatIndicatorTMP.gameObject.SetActive(false);
            combatIndicatorTMP.gameObject.SetActive(true);

            ModSettings.JankTextMeshProUpdaterOnce.Apply(combatIndicatorTMP);

            return true;
        }

        public static string GetStateString(DRAMA_State state)
        {
            string currentState;
            switch (Settings.Style)
            {
                case CombatIndicatorSettings.CombatIndicatorStyle.Simple:
                    if (state < _DRAMA_State_Encounter)
                    {
                        currentState = "OutOfCombat";
                    }
                    else
                    {
                        currentState = "InCombat";
                    }
                    break;
                default:
                case CombatIndicatorSettings.CombatIndicatorStyle.Detailed:
                    currentState = state.ToString();
                    break;
            }
            return currentState;
        }

        [ArchivePatch(typeof(DramaManager), nameof(DramaManager.ChangeState))]
        internal static class DramaManager_ChangeState_Patch
        {
            public static void Postfix(DRAMA_State state)
            {
                if (HasCombatIndicator)
                {
                    CurrentCombatIndicator.text = GetStateString(state);
                    if (!Is.R6OrLater)
                        ModSettings.JankTextMeshProUpdaterOnce.Apply(CurrentCombatIndicator);
                }
            }
        }

        [ArchivePatch(typeof(PUI_LocalPlayerStatus), UnityMessages.Start)]
        internal static class PUI_LocalPlayerStatus_Start_Patch
        {
            public static void Postfix(PUI_LocalPlayerStatus __instance)
            {
                if (GetOrSetupCombatIndicator(__instance, out var ci))
                    CurrentCombatIndicator = ci;
            }
        }
    }
}
