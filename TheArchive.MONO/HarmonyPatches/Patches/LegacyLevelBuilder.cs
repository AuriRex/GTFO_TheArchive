/*#if DEBUG
using Expedition;
using GameData;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class LegacyLevelBuilder
    {

        public static bool CurrentlyGeneratingLevel { get; private set; } = false;

        public static event Action OnLevelGenStart;
        public static event Action OnLevelGenEnd;

        private static List<string> zoneGeomorphTiles = new List<string>();
        private static List<int> zoneGeomorphTilesSubcomplex = new List<int>();
        private static List<int> zoneGeomorphTilesRandomSize = new List<int>();
        private static List<string> randomTiles = new List<string>();
        private static List<int> randomTilesSubcomplex = new List<int>();
        private static List<int> randomTilesRandomSize = new List<int>();

        private static int zoneIndex = 0;
        private static List<string> r1a1_zoneTest = new List<string>()
        {
            "geo_64x64_mining_storage_HA_06",
            "geo_64x64_mining_storage_HA_02",
            "geo_64x64_mining_storage_HA_02",
            "geo_64x64_mining_storage_HA_03",
            "geo_64x64_mining_storage_HA_01",
            "geo_64x64_mining_storage_HA_03"
        };

        private static List<int> r1a1_zoneTest_SC = new List<int>()
        {
            5, 5, 5, 5, 5, 5
        };

        private static List<int> r1a1_zoneTest_RNG = new List<int>()
        {
            7, 7, 7, 7, 7, 7
        };

        private static int randomIndex = 0;
        private static List<string> r1a1_randomTest = new List<string>()
        {
            "geo_32x32_elevator_shaft_mining_03",
            "env_plug_8mheight_elev6m_03_with_gate", // env_plug_8mheight_flat_03_with_gate --> env_plug_8mheight_elev6m_03_with_gate
            "env_plug_8mheight_cap_mining_12",
            "env_plug_8mheight_cap_mining_04",
            "env_plug_8mheight_cap_mining_05",
            "env_plug_8mheight_cap_mining_12",
            "env_plug_8mheight_cap_mining_12",
            "env_plug_8mheight_cap_mining_02",
            "gate_4x4_cap_wall_storage_01",
            "gate_4x4_cap_wall_storage_01",
            "gate_4x4_cap_wall_storage_02",
            "gate_4x4_cap_wall_storage_02",
            "gate_4x4_cap_wall_storage_02",
            "gate_4x4_cap_wall_storage_01",
            "gate_4x4_cap_wall_storage_01",
            "gate_4x4_cap_destroyed_02",
            "gate_8x4_cap_destroyed_01",
            "gate_8x4_cap_destroyed_01",
            "gate_4x4_cap_wall_storage_01",
            "gate_8x4_cap_wall_storage_01",
            "gate_8x4_cap_wall_storage_01",
            "gate_4x4_cap_wall_storage_01",
            "gate_4x4_cap_wall_storage_02",
            "gate_4x4_cap_wall_storage_01",
            "gate_8x4_cap_destroyed_01",
            "gate_4x4_cap_wall_storage_01",
            "gate_4x4_weak_door",
            "gate_8x4_weak_door",
            "gate_8x4_weak_door",
            "gate_8x4_weak_door",
            "gate_8x4_cap_wall_storage_Z_forward_destroyed_01",
            "gate_8x4_cap_wall_storage_Z_forward_destroyed_01",
            "gate_4x4_weak_door",
            "gate_8x4_security_door",
            "gate_4x4_weak_door",
            "gate_8x4_weak_door",
            "gate_8x4_cap_wall_storage_Z_forward_destroyed_01",
            "gate_4x4_weak_door",
            "gate_4x4_security_door",
            "gate_4x4_cap_wall_storage_02",
            "gate_4x4_weak_door",
            "gate_8x4_weak_door",
            "gate_8x4_weak_door",
            "gate_4x4_weak_door",
            "gate_4x4_weak_door",
            "gate_4x4_weak_door",
            "gate_4x4_weak_door",
            "gate_8x4_weak_door",
            "gate_8x4_weak_door",
            "gate_8x4_security_door",
            "gate_4x4_weak_door",
            "gate_8x4_weak_door"
        };

        private static List<int> r1a1_randomTest_SC = new List<int>()
        {
            5, 5, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 5, 5, 5, 5, 2, 2, 5, 5, 5, 5, 2, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5
        };

        private static List<int> r1a1_randomTest_RNG = new List<int>()
        {
            4, 2, 5, 5, 5, 5, 5, 5, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 2, 2, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
        };

        // GS_Generating
        //.Enter()
        [ArchivePatch(typeof(GS_Generating), nameof(GS_Generating.Enter))]
        internal static class GS_Generating_EnterPatch
        {
            public static void Prefix()
            {
                ArchiveLogger.Notice("Level generation started!");
                CurrentlyGeneratingLevel = true;
                OnLevelGenStart?.Invoke();
                zoneGeomorphTiles = new List<string>();
                zoneGeomorphTilesSubcomplex = new List<int>();
                zoneGeomorphTilesRandomSize = new List<int>();
                randomTiles = new List<string>();
                randomTilesSubcomplex = new List<int>();
                randomTilesRandomSize = new List<int>();
                randomIndex = 0;
                zoneIndex = 0;
            }
        }

        [ArchivePatch(typeof(GS_Generating), nameof(GS_Generating.OnBuilderDone))]
        internal static class GS_Generating_OnBuilderDonePatch
        {
            public static void Postfix()
            {
                ArchiveLogger.Notice("Level generation finished!");
                CurrentlyGeneratingLevel = false;
                OnLevelGenEnd?.Invoke();

                OnLevelGenFinished();
            }

            public static void OnLevelGenFinished()
            {
                var zoneGeos = string.Join(", ", zoneGeomorphTiles);
                var zoneGeosSubcplx = string.Join(", ", zoneGeomorphTilesSubcomplex);
                var zoneGeosRngSize = string.Join(", ", zoneGeomorphTilesRandomSize);
                var randomPrefab = string.Join(", ", randomTiles);
                var randomPrefabSubcplx = string.Join(", ", randomTilesSubcomplex);
                var randomPrefabRngSize = string.Join(", ", randomTilesRandomSize);

                ArchiveLogger.Msg(ConsoleColor.DarkMagenta, zoneGeos);
                ArchiveLogger.Msg(ConsoleColor.Magenta, zoneGeosSubcplx);
                ArchiveLogger.Msg(ConsoleColor.DarkGreen, zoneGeosRngSize);
                ArchiveLogger.Msg(ConsoleColor.DarkMagenta, randomPrefab);
                ArchiveLogger.Msg(ConsoleColor.Magenta, randomPrefabSubcplx);
                ArchiveLogger.Msg(ConsoleColor.DarkGreen, randomPrefabRngSize);
            }
        }



        [ArchivePatch(typeof(ComplexResourceSetDataBlock), "GetGeomorphTileFromArray")]
        internal static class ComplexResourceSetDataBlock_GetGeomorphTileFromArrayPatch
        {
            static FieldInfo fi_m_geomorphPickerIndex = typeof(ComplexResourceSetDataBlock).GetField("m_geomorphPickerIndex", AccessTools.all);
            public static bool Prefix(ComplexResourceSetDataBlock __instance, ref GameObject __result, LG_TileShapeType shapeType, GameObject[][] resourceArray, SubComplex subcomplex)
            {
                if(ArchiveMod.CurrentRundown != Utils.RundownID.RundownOne)
                {
                    var array = resourceArray[r1a1_zoneTest_SC[zoneIndex]];

                    foreach (var go in array)
                    {
                        if (go.name == r1a1_zoneTest[zoneIndex])
                        {
                            Builder.BuildSeedRandom.Range(0, r1a1_zoneTest_RNG[zoneIndex]);
                            ArchiveLogger.Success($"Level gen override found: \"{r1a1_zoneTest[zoneIndex]}\" SubComplex: \"{(SubComplex) r1a1_zoneTest_SC[zoneIndex]}\"!");
                            zoneIndex++;
                            __result = go;
                            return false;
                        }
                    }

                    ArchiveLogger.Error($"Level gen override failed to find \"{r1a1_zoneTest[zoneIndex]}\" SubComplex: \"{(SubComplex) r1a1_zoneTest_SC[zoneIndex]}\" in resourceArray!");
                    var list = new List<string>();
                    foreach (var go in array)
                    {
                        list.Add(go.name);
                    }
                    ArchiveLogger.Notice(string.Join(", ", list));
                    zoneIndex++;
                }

                if (resourceArray == null)
                {
                    Debug.LogError("GetGeomorphTileFromArray : ERROR : array is null! : No tile data loaded? " + shapeType);
                    __result = null;
                    return false;
                }
                if (fi_m_geomorphPickerIndex.GetValue(__instance) == null)
                {
                    int valueLength = EnumUtil.GetValueLength<LG_TileShapeType>();
                    fi_m_geomorphPickerIndex.SetValue(__instance, new int[valueLength]);
                }
                int num;
                if (__instance.RandomizeGeomorphOrder)
                {
                    num = Builder.BuildSeedRandom.Range(0, resourceArray[(int) subcomplex].Length, "ComplexResource.RandomizeGeomorphOrder");
                }
                else
                {
                    num = ((int[]) fi_m_geomorphPickerIndex.GetValue(__instance))[(int) shapeType];
                    if (((int[]) fi_m_geomorphPickerIndex.GetValue(__instance))[(int) shapeType] >= resourceArray[(int) subcomplex].Length - 1)
                    {
                        ((int[]) fi_m_geomorphPickerIndex.GetValue(__instance))[(int) shapeType] = 0;
                    }
                    else
                    {
                        ((int[]) fi_m_geomorphPickerIndex.GetValue(__instance))[(int) shapeType]++;
                    }
                }
                __result = resourceArray[(int) subcomplex][num];
                ArchiveLogger.Notice($"GetGeomorphTileFromArrayPatch called: num:{num}, GameObject.name:{__result?.name}, subcomplex:{subcomplex}, shapeType:{shapeType}");
                return false;
            }

            public static void Postfix(ref GameObject __result, GameObject[][] resourceArray, SubComplex subcomplex)
            {
                zoneGeomorphTiles.Add(__result.name);
                zoneGeomorphTilesSubcomplex.Add((int) subcomplex);
                zoneGeomorphTilesRandomSize.Add(resourceArray[(int) subcomplex].Length);
                // 
            }
        }

        //GetCustomGeomorph(eCustomGeomorphType type, int contentIndex = -1, SubComplex subcomplex = SubComplex.All)
        [ArchivePatch(typeof(ComplexResourceSetDataBlock), "GetCustomGeomorph")]
        internal static class ComplexResourceSetDataBlock_GetCustomGeomorphPatch
        {
            public static void Postfix(ref GameObject __result, eCustomGeomorphType type, int contentIndex = -1, SubComplex subcomplex = SubComplex.All)
            {
                ArchiveLogger.Notice($"GetCustomGeomorph called: type:{type}, contentIndex:{contentIndex}, subcomplex:{subcomplex}, resultingGO.name: {__result.name}");
                //return true;
            }
        }

        //GetElevatorTile(SubComplex subcomplex = SubComplex.All)
        [ArchivePatch(typeof(ComplexResourceSetDataBlock), "GetElevatorTile")]
        internal static class ComplexResourceSetDataBlock_GetElevatorTilePatch
        {
            public static void Postfix(ref GameObject __result, SubComplex subcomplex = SubComplex.All)
            {
                ArchiveLogger.Notice($"GetElevatorTile called: subcomplex:{subcomplex}, resultingGO.name: {__result.name}");
                //return true;
            }
        }

        //GetRandomPrefab(GameObject[][] resourceArray, SubComplex subcomplex)
        [ArchivePatch(typeof(ComplexResourceSetDataBlock), "GetRandomPrefab", new Type[] { typeof(GameObject[][]), typeof(SubComplex) })]
        internal static class ComplexResourceSetDataBlock_GetRandomPrefabPatch
        {
            public static bool Prefix(ref GameObject __result, GameObject[][] resourceArray, SubComplex subcomplex)
            {
                try
                {
                    if (ArchiveMod.CurrentRundown != Utils.RundownID.RundownOne)
                    {
                        var array = resourceArray[r1a1_randomTest_SC[randomIndex]];

                        foreach (var go in array)
                        {
                            if (go.name == r1a1_randomTest[randomIndex])
                            {
                                ArchiveLogger.Success($"Level gen override found: \"{r1a1_randomTest[randomIndex]}\" SubcOmplex: \"{(SubComplex) r1a1_randomTest_SC[randomIndex]}\"!");
                                Builder.BuildSeedRandom.Range(0, r1a1_randomTest_RNG[randomIndex]);
                                randomIndex++;
                                __result = go;
                                return false;
                            }
                        }

                        ArchiveLogger.Error($"Level gen override failed to find \"{r1a1_randomTest[randomIndex]}\" SubcOmplex: \"{(SubComplex) r1a1_randomTest_SC[randomIndex]}\" in resourceArray!");
                        var list = new List<string>();
                        foreach(var go in array)
                        {
                            list.Add(go.name);
                        }
                        ArchiveLogger.Notice(string.Join(", ", list));
                        randomIndex++;
                    }
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Error($"{ex}: {ex.Message}");
                    ArchiveLogger.Error(ex.StackTrace);
                }

                return true;
            }

            public static void Postfix(ref GameObject __result, GameObject[][] resourceArray, SubComplex subcomplex)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"GetRandomPrefab called: subcomplex:{subcomplex}, resultingGO.name: {__result.name}");
                //return true;
                randomTiles.Add(__result.name);
                randomTilesSubcomplex.Add((int) subcomplex);
                randomTilesRandomSize.Add(resourceArray[(int) subcomplex].Length);
            }
        }

        //LG_RandomAreaSelector.OnBuild()
        [ArchivePatch(typeof(LG_RandomAreaSelector), "OnBuild")]
        internal static class LG_RandomAreaSelector_OnBuildPatch
        {
            public static bool Prefix(LG_RandomAreaSelector __instance)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"OnBuild called");

                var method = typeof(LG_RandomSelectorBase).GetMethod("OnBuild");
                var ftn = method.MethodHandle.GetFunctionPointer();
                var BaseOnBuild = (Action) Activator.CreateInstance(typeof(Action), __instance, ftn);
                BaseOnBuild();
                if (__instance.m_areas.Length != 0)
                {
                    int num = Builder.BuildSeedRandom.Range(0, __instance.m_areas.Length, "LG_RandomAreaSelector");
                    //Debug.LogError("LG_RandomAreaSelector, OnBuild! r: " + num);
                    for (int i = 0; i < __instance.m_areas.Length; i++)
                    {
                        if (num == i)
                        {
                            __instance.m_areas[i].gameObject.SetActive(true);
                        }
                        else
                        {
                            __instance.m_areas[i].gameObject.SetActive(false);
                        }
                    }
                }
                return false;
            }
        }

    }
}
#endif*/