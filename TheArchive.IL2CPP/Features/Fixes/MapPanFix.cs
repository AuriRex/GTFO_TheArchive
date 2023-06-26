using CellMenu;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Fixes
{
    [EnableFeatureByDefault]
    public class MapPanFix : Feature
    {
        public override string Name => "Map Pan Unclamp";

        public override string Group => FeatureGroups.Fixes;

        public override string Description => "Remove the MMB Map panning restrictions.\nMakes you able to zoom in on far out zones.";

#if IL2CPP
        [ArchivePatch(typeof(CM_PageMap), nameof(CM_PageMap.UpdatePanning))]
        internal static class CM_PageMap_UpdatePanning_Patch
        {
            public static bool Prefix(CM_PageMap __instance)
            {
                if(InputMapper.GetButton.Invoke(InputAction.MapDrag, eFocusState.Map))
                {
                    __instance.m_cursor.SetDragging(true);

                    var pos = __instance.m_mapMover.transform.localPosition;

                    pos.x += __instance.m_mouseInput.x / __instance.m_scaleCurrent;
                    pos.y += __instance.m_mouseInput.y / __instance.m_scaleCurrent;

                    __instance.m_lastMove = pos - __instance.m_mapMover.transform.localPosition;

                    __instance.m_mapMover.transform.localPosition = pos;

                    return ArchivePatch.SKIP_OG;
                }

                return ArchivePatch.RUN_OG;
            }
        }
#else
        [ArchivePatch(typeof(CM_PageMap), "UpdatePanning")]
        internal static class CM_PageMap_UpdatePanning_Patch
        {
            public static float Mathf_Clampnt(float value, float min, float max)
            {
                return value;
            }

            private static MethodInfo _mi_UnityEngine_Mathf_Clamp;
            private static MethodInfo _mi_UnityEngine_Mathf_Clamp_Replacement;

            public static void Init()
            {
                _mi_UnityEngine_Mathf_Clamp = typeof(Mathf).GetMethod(nameof(Mathf.Clamp), Utils.AnyBindingFlagss, null, new Type[] { typeof(float), typeof(float), typeof(float) }, null);
                _mi_UnityEngine_Mathf_Clamp_Replacement = typeof(CM_PageMap_UpdatePanning_Patch).GetMethod(nameof(Mathf_Clampnt), Utils.AnyBindingFlagss);
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if(instruction.opcode == OpCodes.Call && (MethodInfo) instruction.operand == _mi_UnityEngine_Mathf_Clamp)
                    {
                        // Replace calls to UnityEngine.Mathf.Clamp to our custom version
                        yield return new CodeInstruction(OpCodes.Call, _mi_UnityEngine_Mathf_Clamp_Replacement);
                        continue;
                    }

                    yield return instruction;
                }
            }
        }
#endif
    }
}
