using CellMenu;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Accessibility
{
    public class DisableUIParalax : Feature
    {
        public override string Name => "Disable UI Paralax";

        public override string Group => FeatureGroups.Accessibility;

        public override string Description => "Disables the Paralax/Moving of UI elements.\n\n<size=80%>(Only partially works on R4://EXT builds, might fix later.)</size>";

        public override bool SkipInitialOnEnable => true;

        public static bool IsR5Plus { get; private set; }

#if MONO
        public static FieldAccessor<MainMenuGuiLayer, MainMenuGuiLayer> A_MainMenuGuiLayer_Current;
        public static FieldAccessor<MainMenuGuiLayer, CM_PageBase[]> A_MainMenuGuiLayer_m_pages;
#endif
        public override void Init()
        {
#if MONO
            A_MainMenuGuiLayer_Current = FieldAccessor<MainMenuGuiLayer, MainMenuGuiLayer>.GetAccessor("Current");
            A_MainMenuGuiLayer_m_pages = FieldAccessor<MainMenuGuiLayer, CM_PageBase[]>.GetAccessor("m_pages");
#endif
            IsR5Plus = BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownFive.ToLatest());
        }


        public static MainMenuGuiLayer MainMenuGuiLayer_Current
        {
            get
            {
#if IL2CPP
                return MainMenuGuiLayer.Current;
#else
                return A_MainMenuGuiLayer_Current.Get(null);
#endif
            }
        }

        public static CM_PageBase[] MainMenuGuiLayer_AllPages
        {
            get
            {
                var mmgl = MainMenuGuiLayer_Current;
#if IL2CPP
                return mmgl.m_pages;
#else
                return A_MainMenuGuiLayer_m_pages.Get(mmgl);
#endif
            }
        }

        public override void OnEnable()
        {
            if (MainMenuGuiLayer_Current == null) return;
            var pageSettings = MainMenuGuiLayer_Current.GetPage(Utils.GetEnumFromName<eCM_MenuPage>(nameof(eCM_MenuPage.CMP_SETTINGS)));

            if (pageSettings != null)
                pageSettings.m_movingContentHolder.localPosition = Vector3.zero;

            SetPageRundownExpeditionPopupMovementEnabled(false);

            if(!IsR5Plus)
            {
                foreach(var page in MainMenuGuiLayer_AllPages)
                {
                    if (page == null) continue;

                    page.ContenMovementEnabled = false;
                }
            }
        }

        public override void OnDisable()
        {
            SetPageRundownExpeditionPopupMovementEnabled(true);

            if (!IsR5Plus)
            {
                foreach (var page in MainMenuGuiLayer_AllPages)
                {
                    if (page == null) continue;

                    page.ContenMovementEnabled = true;
                }
            }
        }

        public static void SetPageRundownExpeditionPopupMovementEnabled(bool enabled)
        {
            if (MainMenuGuiLayer_Current == null) return;

            var pageRundown = MainMenuGuiLayer_Current?.GetPage(Utils.GetEnumFromName<eCM_MenuPage>(nameof(eCM_MenuPage.CMP_RUNDOWN_NEW)))?.TryCastTo<CM_PageRundown_New>();

            SetPageRundownExpeditionPopupMovementEnabled(pageRundown, enabled);
        }

        // A CM_MovingContentLayer Component is responsible for moving the expedition details window on the rundown screen.
        public static void SetPageRundownExpeditionPopupMovementEnabled(CM_PageRundown_New pageRundown, bool enabled)
        {
            if(pageRundown == null) return;

            var popupMovement = pageRundown.transform.GetChildWithExactName("PopupMovement");
            if (popupMovement != null)
            {
                var movingContentLayer = popupMovement.GetComponent<CM_MovingContentLayer>();
                if(movingContentLayer != null)
                    movingContentLayer.m_moveEvenThoughContentMoveIsDisabled = enabled;

                if(!enabled)
                    popupMovement.localPosition = Vector3.zero;
            }
        }

#if IL2CPP
        // only available in R5 and later
        [RundownConstraint(Utils.RundownFlags.RundownFive, Utils.RundownFlags.Latest)]
        [ArchivePatch(typeof(GuiManager), nameof(GuiManager.GlobalContentMovementEnabled), patchMethodType: ArchivePatch.PatchMethodType.Getter)]
        internal static class GuiManager_GlobalContentMovementEnabled_Patch
        {
            public static void Postfix(ref bool __result)
            {
                __result = false;
            }
        }
#endif

        [ArchivePatch(typeof(CM_PageBase), nameof(CM_PageBase.SetPageActive))]
        internal static class CM_PageBase_SetPageActive_Patch
        {
            public static void Postfix(CM_PageBase __instance)
            {
                if(!IsR5Plus)
                    __instance.ContenMovementEnabled = false;

                __instance.m_movingContentHolder.localPosition = Vector3.zero;
            }
        }

        // PlayerMovement is an extra GameObject on the loadout page that has to be reset.
        [ArchivePatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.SetPageActive))]
        internal static class CM_PageLoadout_SetPageActive_Patch
        {
            public static void Postfix(CM_PageLoadout __instance)
            {
                var playerMovement = __instance.transform.GetChildWithExactName("PlayerMovement");
                if (playerMovement != null)
                {
                    playerMovement.localPosition = Vector3.zero;
                }
            }
        }

        [ArchivePatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.SetPageActive))]
        internal static class CM_PageRundown_New_SetPageActive_Patch
        {
            public static void Postfix(CM_PageRundown_New __instance) => SetPageRundownExpeditionPopupMovementEnabled(__instance, false);
        }

#if MONO
        // We have to patch those two properties as they're overriding the CM_PageBase ones with a constant value of true
        [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFour)]
        [ArchivePatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.ContenMovementEnabled), patchMethodType: ArchivePatch.PatchMethodType.Getter)]
        internal static class CM_PageLoadout_ContenMovementEnabled_Patch
        {
            public static void Postfix(ref bool __result)
            {
                __result = false;
            }
        }

        [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFour)]
        [ArchivePatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.ContenMovementEnabled), patchMethodType: ArchivePatch.PatchMethodType.Getter)]
        internal static class CM_PageRundown_New_ContenMovementEnabled_Patch
        {
            public static void Postfix(ref bool __result)
            {
                __result = false;
            }
        }
#endif
    }
}
