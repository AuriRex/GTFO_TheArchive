using CellMenu;
using GameData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using TMPro;
#if Unhollower
using UnhollowerBaseLib.Attributes;
#endif
#if Il2CppInterop
using Il2CppInterop.Runtime.Attributes;
#endif
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Hud
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownEight, RundownFlags.Latest)]
    public class LogVisualizer : Feature
    {
        public override string Name => "Log Visualizer";

        public override FeatureGroup Group => FeatureGroups.Hud;

        public override string Description => "Missing some logs for <i>that Achievement</i>, huh?";

        public new static ILocalizationService Localization { get; set; }
        
        private static LogVisualizer Instance { get; set; }
        
        [FeatureConfig]
        public static LogVisualizerSettings Settings { get; set; }

        public class LogVisualizerSettings
        {
            [FSHide]
            public bool DebugPrint { get; set; } = false;

            [FSDisplayName("Show Total Log Count")]
            [FSDescription("Shows overall log count and per rundown count under the title at the top after selecting one.")]
            public bool ShowTotalCount { get; set; } = true;

            [FSDisplayName("Show Log Count for Rundowns")]
            [FSDescription("Shows the total count for each rundown on the rundown selection menu.")]
            public bool ShowCountPerRundownSelectionButton { get; set; } = true;

            [FSDisplayName("Show Log Count on Expeditions")]
            [FSDescription("Shows the total count of logs in each expedition.")]
            public bool ShowCountPerExpedition { get; set; } = true;

            [FSHeader(":// In Expedition Display")]
            [FSDisplayName("Enable")]
            [FSDescription("Shows you the logs available in the current expedition on the Objectives screen.")]
            public bool ShowInExpeditionDisplay { get; set; } = true;

            [FSDisplayName("Show Zone Spoiler")]
            [FSDescription("Displays the Zone a log is in.")]
            public SpoilerMode Spoilers { get; set; } = SpoilerMode.Never;

            [FSHeader(":// Colors")]
            [FSDisplayName("Color: Logs Incomplete")]
            public SColor ColorNormal { get; set; } = SColor.WHITE;

            [FSDisplayName("Color: All Logs Gotten")]
            public SColor ColorAllGotten { get; set; } = SColor.ORANGE;

            [FSDisplayName("Color: No Logs Available")]
            public SColor ColorNoLogs { get; set; } = new SColor(0.5f, 0.5f, 0.5f);

            [Localized]
            public enum SpoilerMode
            {
                Never,
                OnceDiscovered,
                Always
            }
        }

        public override bool ShouldInit()
        {
            return !IsPlayingModded;
        }

        public override void Init()
        {
            LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<CM_LogDisplayRoot>();
            LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<CM_LogDisplayItem>();
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            if (setting.Identifier.EndsWith(nameof(Settings.ShowInExpeditionDisplay))
                || setting.Identifier.EndsWith(nameof(Settings.Spoilers)))
            {
                OnLogDisplaySettingChanged();
            }
        }

        private void OnLogDisplaySettingChanged()
        {
            var createdThisFrame = GetOrCreateLogDisplayRoot(MainMenuGuiLayer.Current?.PageObjectives?.m_artifactInventoryDisplay, out var logDisplay);

            if (Settings.ShowInExpeditionDisplay)
            {
                InitializeAndEnableLogDisplayForActiveExpedition(logDisplay, createdThisFrame);
            }
            else
            {
                logDisplay.DisableDisplay();
            }
        }

        private static readonly eGameStateName _eGameStateName_InLevel = GetEnumFromName<eGameStateName>(nameof(eGameStateName.InLevel));

        public override void OnEnable()
        {
            if (!DataBlocksReady)
                return;

            TryDiscoverAllLogsFromDataBlocks();

            if (!Settings.ShowInExpeditionDisplay)
                return;

            var createdThisFrame = GetOrCreateLogDisplayRoot(MainMenuGuiLayer.Current?.PageObjectives?.m_artifactInventoryDisplay, out var logDisplay);

            if (logDisplay == null)
            {
                FeatureLogger.Warning($"Log Display is null! (This shouldn't happen!)");
            }

            if((eGameStateName)CurrentGameState == _eGameStateName_InLevel)
            {
                // We are in level, update log display
                InitializeAndEnableLogDisplayForActiveExpedition(logDisplay, createdThisFrame);
            }
        }

        public void OnGameStateChanged(eGameStateName state)
        {
            if(state == _eGameStateName_InLevel && Settings.ShowInExpeditionDisplay)
            {
                var createdThisFrame = GetOrCreateLogDisplayRoot(MainMenuGuiLayer.Current?.PageObjectives?.m_artifactInventoryDisplay, out var logDisplay);
                InitializeAndEnableLogDisplayForActiveExpedition(logDisplay, createdThisFrame);
            }
        }

        public override void OnDisable()
        {
            DisableLogDisplayRoot();
            // TODO: Cleanup Log texts on RundownScreen(s)
        }

        private void InitializeAndEnableLogDisplayForActiveExpedition(CM_LogDisplayRoot logDisplay, bool createdThisFrame)
        {
            var expedition = RundownManager.GetActiveExpeditionData();

            var expIndex = expedition.expeditionIndex;
            var expTier = expedition.tier;
            var rundownKey = expedition.rundownKey.data;

            FeatureLogger.Debug($"Initializing LogDisplay for expedition: Rundown: {rundownKey}, Tier: {expTier}, Index: {expIndex}");

            var logs = GetLogsForExpedition(rundownKey, expTier, expIndex);

            if (!createdThisFrame)
            {
                logDisplay.Initialize(logs);
                logDisplay.EnableDisplay();
                return;
            }

            // Turns out waiting one frame after cloning something fixes weird behaviour with old values/GOs persisting
            LoaderWrapper.StartCoroutine(NextFrame(() =>
            {
                logDisplay.Initialize(logs);
                logDisplay.EnableDisplay();
            }));
        }

        public new static IArchiveLogger FeatureLogger { get; set; }

        public record class LogInExpedition
        {
            public uint Rundown { get; set; }
            public uint ExpeditionTier { get; set; }
            public uint ExpeditionNumber { get; set; }
            public uint ExpeditionIndex { get; set; }
            public string LogFileName { get; set; }
            public uint LogId { get; set; }
            public bool IsAudioLog { get; set; }
            public int DimensionIndex { get; internal set; }
            public int Zone { get; internal set; }
            public int ZoneOverride { get; internal set; }

            public override string ToString()
            {
                var rd = RundownDataBlock.GetBlock(Rundown);
                return $"{rd?.StorytellingData?.Title?.ToString().Split('\n')[1].Replace("TITLE: ", string.Empty)}: {char.ConvertFromUtf32(65 + (int)ExpeditionTier)}{ExpeditionNumber + 1}: {LogFileName} | ID: {LogId} | Audio: {IsAudioLog} | {(eDimensionIndex) DimensionIndex} | ZONE_{(ZoneOverride >= 0 ? ZoneOverride : Zone)}";
            }
        }

        public override void OnDatablocksReady()
        {
            TryDiscoverAllLogsFromDataBlocks();
        }

        private static bool _logsDiscovered = false;
        private void TryDiscoverAllLogsFromDataBlocks()
        {
            if (_logsDiscovered)
                return;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var setupDB = GameSetupDataBlock.GetAllBlocks()[0];

            foreach (var loadedRundownDBIDs in setupDB.RundownIdsToLoad)
            {
                var rundownDB = RundownDataBlock.GetBlock(loadedRundownDBIDs);

                if (rundownDB == null)
                    return;

                var rundownID = rundownDB.persistentID;

                DataBlockTraversal.IterateTier(rundownDB.TierA, rundownID, 0);
                DataBlockTraversal.IterateTier(rundownDB.TierB, rundownID, 1);
                DataBlockTraversal.IterateTier(rundownDB.TierC, rundownID, 2);
                DataBlockTraversal.IterateTier(rundownDB.TierD, rundownID, 3);
                DataBlockTraversal.IterateTier(rundownDB.TierE, rundownID, 4);
            }

            _allLogs = _allLogs.Distinct().ToList();

            _logsDiscovered = true;

            /*foreach (var log in _allLogs)
            {
                FeatureLogger.Info($"{log}");
            }*/

            stopwatch.Stop();

            FeatureLogger.Success($"Discovered {_allLogs.Count} total log files in DataBlocks in {stopwatch.Elapsed}");
        }

        #region DataBlockTraversal
        private static class DataBlockTraversal
        {
            public static void IterateTier(Il2CppSystem.Collections.Generic.List<ExpeditionInTierData> tiers, uint rundownID, uint expTier)
            {
                if (tiers == null)
                    return;

                uint expNumber = 0;
                uint expIndex = 0;
                foreach(var expedition in tiers)
                {
                    if (expedition == null)
                        return;

                    IterateLayer(expedition.LevelLayoutData, rundownID, expTier, expNumber, expIndex, eDimensionIndex.Reality);
                    IterateLayer(expedition.SecondaryLayout, rundownID, expTier, expNumber, expIndex, eDimensionIndex.Reality);
                    IterateLayer(expedition.ThirdLayout, rundownID, expTier, expNumber, expIndex, eDimensionIndex.Reality);

                    foreach(var dimensionData in expedition.DimensionDatas)
                    {
                        var dimensionDB = DimensionDataBlock.GetBlock(dimensionData.DimensionData);

                        var dimensionIndex = dimensionData.DimensionIndex;

                        if(dimensionDB.DimensionData.StaticTerminalPlacements != null)
                        {
                            foreach (var term in dimensionDB.DimensionData.StaticTerminalPlacements)
                            {
                                if (term.LocalLogFiles == null)
                                    continue;

                                foreach (var logFile in term.LocalLogFiles)
                                {
                                    if (logFile.FileContent.Id == 0)
                                        continue;

                                    var info = new LogInExpedition
                                    {
                                        Rundown = rundownID,
                                        ExpeditionTier = expTier,
                                        ExpeditionNumber = expNumber,
                                        ExpeditionIndex = expIndex,
                                        LogFileName = logFile.FileName,
                                        LogId = logFile.FileContent.Id,
                                        IsAudioLog = logFile.AttachedAudioFile != 0,
                                        DimensionIndex = (int)dimensionIndex,
                                        Zone = 0,
                                        ZoneOverride = 0,
                                    };

                                    AddLog(info);
                                }
                            }
                        }

                        IterateLayer(dimensionDB.DimensionData.LevelLayoutData, rundownID, expTier, expNumber, expIndex, dimensionIndex);
                    }

                    if(expedition.Accessibility != eExpeditionAccessibility.AlwayBlock)
                        expNumber++;

                    expIndex++;
                }
            }

            private static void IterateLayer(uint layoutDataId, uint rundown, uint tier, uint expeditionNum, uint expeditionIndex, eDimensionIndex dimensionIndex)
            {
                if (layoutDataId == 0)
                    return;

                var level = LevelLayoutDataBlock.GetBlock(layoutDataId);

                if (level == null)
                    return;

                foreach(var zone in level.Zones)
                {
                    if(zone.SpecificTerminalSpawnDatas != null)
                    {
                        foreach (var term in zone.SpecificTerminalSpawnDatas)
                        {
                            TerminalToLogs(level, zone, term, rundown, tier, expeditionNum, expeditionIndex, dimensionIndex);
                        }
                    }
                
                    if(zone.TerminalPlacements != null)
                    {
                        foreach (var term in zone.TerminalPlacements)
                        {
                            TerminalToLogs(level, zone, term, rundown, tier, expeditionNum, expeditionIndex, dimensionIndex);
                        }
                    }
                
                }
            }

            private static void TerminalToLogs(LevelLayoutDataBlock level, ExpeditionZoneData zone, TerminalPlacementData term, uint rundown, uint tier, uint expeditionNum, uint expeditionIndex, eDimensionIndex dimensionIndex)
            {
                if (term?.LocalLogFiles == null)
                    return;

                foreach (var logFile in term.LocalLogFiles)
                {
                    if (logFile.FileContent.Id == 0)
                        continue;

                    var info = new LogInExpedition
                    {
                        Rundown = rundown,
                        ExpeditionTier = tier,
                        ExpeditionNumber = expeditionNum,
                        ExpeditionIndex = expeditionIndex,
                        LogFileName = logFile.FileName,
                        LogId = logFile.FileContent.Id,
                        IsAudioLog = logFile.AttachedAudioFile != 0,
                        DimensionIndex = (int)dimensionIndex,
                        Zone = zone.Alias == 0 ? level.ZoneAliasStart + (int)zone.LocalIndex : zone.Alias,
                        ZoneOverride = zone.AliasOverride,
                    };

                    AddLog(info);
                }
            }
        }
        #endregion DataBlockTraversal

        #region LogsRelateStuff
        private static List<LogInExpedition> _allLogs = new();
        // RundownID
        private static readonly Dictionary<uint, List<LogInExpedition>> _rundownLogLookup = new();
        // RundownID, Tier, ExpeditionIndex
        private static readonly Dictionary<uint, Dictionary<uint, Dictionary<uint, List<LogInExpedition>>>> _logLookup = new();

        private static void AddLog(LogInExpedition log)
        {
            _allLogs.Add(log);

            if(!_rundownLogLookup.TryGetValue(log.Rundown, out var rundownList))
            {
                rundownList = new List<LogInExpedition>();
                _rundownLogLookup.Add(log.Rundown, rundownList);
            }

            rundownList.Add(log);


            if (!_logLookup.TryGetValue(log.Rundown, out var tierDict))
            {
                tierDict = new();
                _logLookup.Add(log.Rundown, tierDict);
            }

            if (!tierDict.TryGetValue(log.ExpeditionTier, out var expeditionDict))
            {
                expeditionDict = new();
                tierDict.Add(log.ExpeditionTier, expeditionDict);
            }

            if (!expeditionDict.TryGetValue(log.ExpeditionIndex, out var logList))
            {
                logList = new List<LogInExpedition>();
                expeditionDict.Add(log.ExpeditionIndex, logList);
            }

            logList.Add(log);
        }

        private static readonly IReadOnlyList<LogInExpedition> _emptyList = new List<LogInExpedition>();
        public static IReadOnlyList<LogInExpedition> GetAllLogsFromExpedition(uint rundownID, uint expTier, uint expIndex)
        {
            if (_logLookup.TryGetValue(rundownID, out var tierDict)
                && tierDict.TryGetValue(expTier, out var expeditionDict)
                && expeditionDict.TryGetValue(expIndex, out var logList))
                return logList;

            return _emptyList;
        }

        public static IReadOnlyList<LogInExpedition> GetAllLogsFromRundown(uint rundownID)
        {
            if (_rundownLogLookup.TryGetValue(rundownID, out var logList))
                return logList;

            return _emptyList;
        }

        private static readonly ISet<LogEntry> _emptyLogSet = new HashSet<LogEntry>();
        public ISet<LogEntry> GetLogsForExpedition(string rundownKey, eRundownTier eExpTier, int expIndex)
        {
            var expTier = (uint)eExpTier - 1;

            if (!TryParseRundownKey(rundownKey, out var rundownID))
            {
                FeatureLogger.Warning($"Failed to parse Rundown ID from \"{rundownKey}\" (Tier: {expTier}, Index: {expIndex})");
                return _emptyLogSet;
            }

            var ral = GetReadAllLogsAchievementInstance();

            if (ral == null || ral.m_allLogs == null || ral.m_readLogsOnStart == null)
                return _emptyLogSet;

            var logsForExpedition = GetAllLogsFromExpedition(rundownID, expTier, (uint)expIndex);
            var logsIHave = logsForExpedition.Where(log => ral.m_readLogsOnStart.Contains(log.LogId));

            var logsForExpeditionDistinct = logsForExpedition.Select(log => log.LogId).Distinct().ToArray();
            var logsIHaveDistinct = logsIHave.Select(log => log.LogId).Distinct().ToArray();

            var set = new HashSet<LogEntry>();
            foreach(var id in logsForExpeditionDistinct)
            {
                var log = logsForExpedition.FirstOrDefault(log => log.LogId == id);

                if (log == null)
                    continue;

                set.Add(new LogEntry(id, log.LogFileName, collected: logsIHaveDistinct.Any(lih => lih == id), log));
            }

            return set;
        }
        #endregion LogsRelateStuff

        public static Achievement_ReadAllLogs GetReadAllLogsAchievementInstance()
        {
            if (AchievementManager.Current?.m_allAchievements == null)
                return null;

            foreach (var ach in AchievementManager.Current.m_allAchievements)
            {
                if (ach.GetIl2CppType().Name == nameof(Achievement_ReadAllLogs))
                    return ach.TryCastTo<Achievement_ReadAllLogs>();
            }

            return null;
        }

        [ArchivePatch(typeof(Achievement_ReadAllLogs), nameof(Achievement_ReadAllLogs.OnPlayFabLoginSuccess))]
        internal static class Achievement_ReadAllLogs_OnPlayFabLoginSuccess_Patch
        {
            public static void Postfix(Achievement_ReadAllLogs __instance)
            {
                if (__instance.m_allLogs == null)
                {
                    FeatureLogger.Notice($"Achievements are likely disabled, disabling {nameof(LogVisualizer)}!");
                    Instance?.RequestDisable("Achievements likely disabled");
                    return;
                }
                
                var allLogsForAchievement = _allLogs.Where(log => __instance.m_allLogs.Contains(log.LogId)).ToList();
                var allLogsForAchievementDistinct = allLogsForAchievement.Select(log => log.LogId).Distinct().ToArray();

                var logsIhave = allLogsForAchievement.Where(log => __instance.m_readLogsOnStart.Contains(log.LogId));
                var logsIHaveDistinct = logsIhave.Select(log => log.LogId).Distinct().ToArray();

                FeatureLogger.Notice($"{logsIHaveDistinct.Length}/{allLogsForAchievementDistinct.Length} Logs found!");

                if (!Settings.DebugPrint)
                    return;

                FeatureLogger.Notice("Printing all obtained logs:");
                foreach(var log in logsIhave)
                {
                    FeatureLogger.Info($"{log}");
                }
                FeatureLogger.Success("---");

                FeatureLogger.Fail("Logs not required for Achievement:");
                var logsThatArentAchievementRequired = _allLogs.Where(log => !__instance.m_allLogs.Contains(log.LogId)).ToList();
                foreach (var logInfo in logsThatArentAchievementRequired)
                {
                    FeatureLogger.Success(logInfo.ToString());
                }

                var logsThatCouldntBeFoundInLevels = __instance.m_allLogs.ToSystemList().Where(id => !_allLogs.Any(log => log.LogId == id));

                FeatureLogger.Fail("Logs that weren't found in any level?:");
                foreach(var logId in logsThatCouldntBeFoundInLevels)
                {
                    FeatureLogger.Notice($"Missing Log: {logId}");
                }
            }
        }

        private static readonly string[] _br = new string[] { "<br>" };

        [ArchivePatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.UpdateHeaderText))]
        internal static class CM_PageRundown_New_UpdateHeaderText_Patch
        {
            public static void Postfix(CM_PageRundown_New __instance)
            {
                //FeatureLogger.Debug($"{nameof(CM_PageRundown_New_UpdateHeaderText_Patch)} called.");

                if (!Settings.ShowTotalCount)
                    return;

                var ogHeader = __instance.m_textRundownHeader;

                var currentRundownID = __instance.m_currentRundownData?.persistentID ?? 0;

                var title = ogHeader.text.Contains("<br>") ? ogHeader.text.Split(_br, StringSplitOptions.None)[0] : ogHeader.text;

                if (string.IsNullOrWhiteSpace(title))
                    title = __instance.m_customHeader;

                var rel = GetReadAllLogsAchievementInstance();

                if (rel == null || rel.m_allLogs == null || rel.m_readLogsOnStart == null)
                    return;

                var countTotal = 0;
                var countReadLogs = 0;

                if(currentRundownID == 0)
                {
                    countTotal = rel.m_allLogs.Count;
                    countReadLogs = rel.m_readLogsOnStart.Count;
                }
                else
                {
                    var allLogsFromRundown = GetAllLogsFromRundown(currentRundownID).Where(log => rel.m_allLogs.Contains(log.LogId));
                    var allLogsFromRundownIHave = allLogsFromRundown.Where(log => rel.m_readLogsOnStart.Contains(log.LogId));

                    var allLogsDistinct = allLogsFromRundown.Select(log => log.LogId).Distinct().ToArray();
                    var allLogsIHaveDistinct = allLogsFromRundownIHave.Select(log => log.LogId).Distinct().ToArray();

                    countTotal = allLogsDistinct.Length;
                    countReadLogs = allLogsIHaveDistinct.Length;
                }

                __instance.m_textRundownHeader.SetText($"{title}<br><size=70%>Total Logs: ({countReadLogs} / {countTotal})</size>");
            }
        }

        //UpdateRundownSelectionButton(CM_RundownSelection rundownSelection, System.UInt32 rundownId)
        [ArchivePatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.UpdateRundownSelectionButton))]
        internal static class CM_PageRundown_New_UpdateRundownSelectionButton_Patch
        {
            public static readonly Vector3 _og_friendsHostingTextPos = new Vector3(0, 16.4f, 0);
            public static readonly Vector3 _new_friendsHostingTextPos = new Vector3(0, 76.4f, 0); // Moves it above

            public static void Postfix(CM_RundownSelection rundownSelection, int rundownId)
            {
                //FeatureLogger.Debug($"Updating Rundown Selection Logs Text for Rundown ID \"{rundownId}\" ...");

                if (!Settings.ShowCountPerRundownSelectionButton)
                    return;

                var text = rundownSelection.m_rundownText;

                if (text == null)
                    return;

                var rundownText = text.text.Contains("<br>") ? text.text.Split(_br, StringSplitOptions.None)[0] : text.text;

                var ral = GetReadAllLogsAchievementInstance();

                if (ral == null || ral.m_allLogs == null || ral.m_readLogsOnStart == null)
                    return;

                var logsForRundown = GetAllLogsFromRundown((uint)rundownId).Where(log => ral.m_allLogs.Contains(log.LogId));
                var logsIHave = logsForRundown.Where(log => ral.m_readLogsOnStart.Contains(log.LogId));

                var logsForRundownDistinct = logsForRundown.Select(log => log.LogId).Distinct().ToArray();
                var logsIHaveDistinct = logsIHave.Select(log => log.LogId).Distinct().ToArray();


                SColor color = Settings.ColorNormal;
                var logCountText = $"{logsIHaveDistinct.Length} / {logsForRundownDistinct.Length}";

                if(logsForRundownDistinct.Length == 0)
                {
                    color = Settings.ColorNoLogs;
                    logCountText = Localization.Get(1);
                }
                else if(logsIHaveDistinct.Length == logsForRundownDistinct.Length)
                {
                    color = Settings.ColorAllGotten;
                }

                
                var logsText = $"<br><size=60%><{color.ToHexString()}>{Localization.Get(2)}: ({logCountText})</color></size>";

                rundownSelection.m_rundownText.SetText($"{rundownText}{logsText}");

                rundownSelection.m_hostingFriendsCountText.transform.localPosition = _new_friendsHostingTextPos;

                if(rundownSelection.m_altText != null)
                {
                    if(!rundownSelection.m_altText.text.Contains("<br>"))
                    {
                        // Push up alt text so it doesn't look like ass
                        rundownSelection.m_altText.SetText($"{rundownSelection.m_altText.text}<br><br><size=60%><alpha=#00>.</alpha></size>");
                    }
                }

            }
        }

        [ArchivePatch(typeof(CM_ExpeditionIcon_New), nameof(CM_ExpeditionIcon_New.SetStatus))]
        internal static class CM_ExpeditionIcon_New_SetStatus_Patch
        {
            public static readonly Vector3 _og_friendsHostingTextPos = new Vector3(-156.3781f, -99.4f, -1.3656f);
            public static readonly Vector3 _new_friendsHostingTextPos = new Vector3(-156.3781f, -124.4f, -1.3656f);
            //-156.3781 -124.4 -1.3656

            public static void Postfix(CM_ExpeditionIcon_New __instance)
            {
                //FeatureLogger.Debug($"{nameof(CM_ExpeditionIcon_New)}{nameof(CM_ExpeditionIcon_New.SetStatus)}() called for \"{__instance.FullName}\".");

                if (!Settings.ShowCountPerExpedition)
                    return;

                var originalStatus = __instance.m_statusText.text.Contains("<br>") ? __instance.m_statusText.text.Split(_br, StringSplitOptions.None)[0] : __instance.m_statusText.text;

                var ral = GetReadAllLogsAchievementInstance();

                if (ral == null || ral.m_allLogs == null || ral.m_readLogsOnStart == null)
                    return;

                var expIndex = (uint)__instance.ExpIndex;
                var expTier = (uint)__instance.Tier - 1;

                
                if(!TryParseRundownKey(__instance.RundownKey, out var rundownID))
                {
                    FeatureLogger.Warning($"Failed to parse Rundown ID from \"{__instance.RundownKey}\" ({__instance.FullName})");
                    return;
                }

                __instance.m_hostingFriendsCountText.transform.localPosition = _new_friendsHostingTextPos;

                
                var logsForExpedition = GetAllLogsFromExpedition(rundownID, expTier, expIndex).Where(log => ral.m_allLogs.Contains(log.LogId));
                var logsIHave = logsForExpedition.Where(log => ral.m_readLogsOnStart.Contains(log.LogId));

                var logsForExpeditionDistinct = logsForExpedition.Select(log => log.LogId).Distinct().ToArray();
                var logsIHaveDistinct = logsIHave.Select(log => log.LogId).Distinct().ToArray();

                SColor color = Settings.ColorNormal;
                string logsText = $"{logsIHaveDistinct.Length} / {logsForExpeditionDistinct.Length}";

                if (logsForExpeditionDistinct.Length == 0)
                {
                    logsText = Localization.Get(1);
                    color = Settings.ColorNoLogs;
                }
                else if(logsIHaveDistinct.Length == logsForExpeditionDistinct.Length)
                {
                    color = Settings.ColorAllGotten;
                }

                __instance.m_statusText.SetText($"{originalStatus}<br><{color.ToHexString()}>{Localization.Get(2)}: ({logsText})</color>");
            }
        }

        #region ObjectivesScreen

        public class LogEntry
        {
            public readonly uint logId;
            public readonly string logName;
            public readonly bool collected;
            public readonly LogInExpedition details;

            public LogEntry(uint id, string logFileName, bool collected, LogInExpedition details)
            {
                logId = id;
                logName = logFileName;
                this.collected = collected;
                this.details = details;
            }
        }

        public class CM_LogDisplayRoot : MonoBehaviour
        {
            public CM_LogDisplayRoot(IntPtr ptr) : base(ptr) { }

            public Vector3 LocalItemRootPos { get; internal set; }
            public static Vector3 ItemDistance { get; private set; } = new Vector3(0, -45, 0);

            public TextMeshPro HeaderText { get; internal set; }
            //public static GameObject ItemPrefab { get; internal set; }

            public static string PrefabGOName { get; internal set; }


            private readonly List<CM_LogDisplayItem> _items = new();

            public void EnableDisplay()
            {
                gameObject.SetActive(true);
            }

            public void DisableDisplay()
            {
                gameObject.SetActive(false);
            }

            public void SetLogCollected(uint logId)
            {
                foreach(var item in _items)
                {
                    if(item.LogID == logId)
                    {
                        item.SetCollected(true);
                    }
                }

                UpdateHeaderText();
            }

            [HideFromIl2Cpp]
            public void Initialize(ISet<LogEntry> logs)
            {
                Cleanup();

                if(logs.Count > 0)
                {
                    foreach (var entry in logs)
                    {
                        string spoilerText;
                        if(entry.details.Zone == 0 && entry.details.ZoneOverride == 0)
                        {
                            spoilerText = $"DIM_{entry.details.DimensionIndex}";
                        }
                        else
                        {
                            spoilerText = $"ZONE_{(entry.details.ZoneOverride > 0 ? entry.details.ZoneOverride : entry.details.Zone)}";

                            if (((eDimensionIndex)entry.details.DimensionIndex) != eDimensionIndex.Reality)
                            {
                                spoilerText = $"{spoilerText}, DIM_{entry.details.DimensionIndex}";
                            }
                        }
                        CreateItem(entry.logId, entry.logName, entry.collected, spoilerText);
                    }
                }

                UpdateHeaderText();
            }

            private void UpdateHeaderText()
            {
                if (_items.Count == 0)
                {
                    HeaderText.SetText(Localization.Get(3));
                    return;
                }

                var collectedAmount = _items.Where(item => item.Collected).Count();

                HeaderText.SetText($"{Localization.Get(2)}: ({collectedAmount} / {_items.Count})");
            }

            public void UpdateItemVisuals()
            {
                foreach (var item in _items)
                {
                    item.UpdateVisuals();
                }
            }

            private void CreateItem(uint logid, string logName, bool collected, string spoiler)
            {
                var offset = _items.Count * ItemDistance;

                var instance = Instantiate(transform.GetChildWithExactName(PrefabGOName).gameObject);

                instance.transform.SetParent(transform, false);

                instance.transform.localPosition = LocalItemRootPos + offset;

                var logDisplayItem = instance.GetComponent<CM_LogDisplayItem>();

                // For some reason those fields aren't saved through Instantiate() so we have to find the components again ...
                logDisplayItem.Text ??= instance.transform.GetChildWithExactName("CategoryHeader").GetComponent<TextMeshPro>();
                logDisplayItem.Divider ??= instance.transform.GetChildWithExactName("DividerLine").GetComponent<SpriteRenderer>();
                logDisplayItem.Gradient ??= instance.transform.GetChildWithExactName("Gradient").GetComponent<SpriteRenderer>();

                logDisplayItem.LogID = logid;
                logDisplayItem.LogName = logName;
                logDisplayItem.Spoiler = spoiler;

                logDisplayItem.SetCollected(collected, true);

                instance.gameObject.SetActive(true);

                _items.Add(logDisplayItem);
            }

            public void Cleanup()
            {
                foreach(var item in _items)
                {
                    item.SafeDestroyGO();
                }

                _items.Clear();
            }
        }

        public class CM_LogDisplayItem : MonoBehaviour
        {
            public CM_LogDisplayItem(IntPtr ptr) : base(ptr) { }

            public TextMeshPro Text;
            public SpriteRenderer Divider;
            public SpriteRenderer Gradient;

            public bool Collected { get; private set; }
            public bool CollectedOnStart { get; private set; }
            public uint LogID { get; internal set; } = 0;
            public string LogName { get; internal set; } = string.Empty;
            public string Spoiler { get; internal set; }

            public void SetCollected(bool collected, bool alreadyCollectedOnLevelStart = false)
            {
                Collected = collected;
                CollectedOnStart = alreadyCollectedOnLevelStart;

                UpdateVisuals();
            }

            public void UpdateVisuals()
            {
                string specialText = string.Empty;

                if (Settings.Spoilers == LogVisualizerSettings.SpoilerMode.Always
                    || (Settings.Spoilers == LogVisualizerSettings.SpoilerMode.OnceDiscovered && Collected))
                {
                    specialText = $" <#999><size=75%>[{Spoiler}]</size></color>";
                }

                if (Collected)
                {
                    Text.SetText($"{LogName}{specialText}");
                    Text.color = Color.white;
                    Divider.color = Color.green;

                    if(CollectedOnStart)
                    {
                        Gradient.color = Color.white.WithAlpha(0.025f);
                        return;
                    }

                    Gradient.color = Color.green.WithAlpha(0.025f);
                    return;
                }

                Text.SetText($"{Localization.Get(4)}{specialText}");
                Text.color = Color.gray;
                Divider.color = Color.red;
                Gradient.color = Color.red.WithAlpha(0.025f);
            }
        }

        private static CM_LogDisplayRoot _logDisplayRoot = null;

        private static void DisableLogDisplayRoot()
        {
            _logDisplayRoot?.DisableDisplay();
        }

        /// <summary>
        /// Gets or creates the Log Display UI elements for the <seealso cref="CM_PageObjectives"/> screen
        /// </summary>
        /// <param name="artifactInvDisplay">The <seealso cref="CM_ArtifactInventoryDisplay"/> to clone</param>
        /// <param name="logDisplayRoot">Newly created <seealso cref="CM_LogDisplayRoot"/> object</param>
        /// <param name="isFromStartup">Set to true if called from startup, otherwise ignore</param>
        /// <returns>If the object was created this frame</returns>
        public static bool GetOrCreateLogDisplayRoot(CM_ArtifactInventoryDisplay artifactInvDisplay, out CM_LogDisplayRoot logDisplayRoot, bool isFromStartup = false)
        {
            if (artifactInvDisplay == null || _logDisplayRoot != null)
            {
                logDisplayRoot = _logDisplayRoot;
                return false;
            }

            var rootGO = GameObject.Instantiate(artifactInvDisplay.gameObject);

            rootGO.transform.SetParent(artifactInvDisplay.transform.parent, false);

            var basePosition = rootGO.transform.localPosition;

            var x = isFromStartup ? basePosition.x - 100 : basePosition.x; // idk

            rootGO.transform.localPosition = new Vector3(x, -50, basePosition.z);

            var newArtifactInvDisplay = rootGO.GetComponent<CM_ArtifactInventoryDisplay>();

            rootGO.name = "LogDisplayRoot";

            var artifactCounter = newArtifactInvDisplay.m_basicCounter;

            artifactCounter.m_artifactsTitle.SafeDestroyGO();
            artifactCounter.m_artifactValue.SafeDestroyGO();
            artifactCounter.m_boosterValueTitle.SafeDestroyGO();
            artifactCounter.m_boosterValue.SafeDestroyGO();

            var counterHeader = artifactCounter.m_categoryHeader;
            var dividerLine = artifactCounter.DividerLine;
            var gradient = artifactCounter.Gradient;

            newArtifactInvDisplay.m_basicCounter.SafeDestroy();
            newArtifactInvDisplay.m_specializedCounter.SafeDestroyGO();
            newArtifactInvDisplay.m_advancedCounter.SafeDestroyGO();
            newArtifactInvDisplay.SafeDestroy();

            dividerLine.transform.localScale = new Vector3(1, 1f / 3f, 1);
            dividerLine.transform.localPosition = dividerLine.transform.localPosition + new Vector3(0, 30, 0);

            gradient.transform.localScale = new Vector3(1, 1f / 3f, 1);
            gradient.transform.localPosition = gradient.transform.localPosition + new Vector3(0, 30, 0);
            gradient.gameObject.SetActive(true);

            artifactCounter.name = "LogDisplayItem";

            logDisplayRoot = rootGO.AddComponent<CM_LogDisplayRoot>();

            counterHeader.SetText("COO-LXL-OGS");
            artifactCounter.gameObject.SetActive(false);
            var logItemPrefab = artifactCounter.gameObject.AddComponent<CM_LogDisplayItem>();

            logItemPrefab.Text = counterHeader;
            logItemPrefab.Divider = dividerLine;
            logItemPrefab.Gradient = gradient;

            var headerText = rootGO.transform.GetChildWithExactName("HeaderText").GetComponent<TextMeshPro>();
            headerText.GetComponent<TMP_Localizer>().SafeDestroy();
            headerText.SetText(Localization.Get(2));

            logDisplayRoot.HeaderText = headerText;
            logDisplayRoot.LocalItemRootPos = logItemPrefab.transform.localPosition;
            CM_LogDisplayRoot.PrefabGOName = logItemPrefab.gameObject.name;

            _logDisplayRoot = logDisplayRoot;
            rootGO.SetActive(false);

            UnityEngine.Object.DontDestroyOnLoad(_logDisplayRoot.gameObject);
            _logDisplayRoot.gameObject.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(_logDisplayRoot);
            _logDisplayRoot.hideFlags = HideFlags.HideAndDontSave;

            return true;
        }

        [ArchivePatch(typeof(Achievement_ReadAllLogs), nameof(Achievement_ReadAllLogs.OnReadLog))]
        internal static class Achievement_ReadAllLogs_OnReadLog_Patch
        {
            public static void Postfix(LevelGeneration.pLogRead data)
            {
                FeatureLogger.Info($"Log with ID \"{data.ID}\" read.");
                _logDisplayRoot?.SetLogCollected(data.ID);
            }
        }

        [ArchivePatch(typeof(CM_ArtifactInventoryDisplay), nameof(CM_ArtifactInventoryDisplay.Setup))]
        internal static class CM_ArtifactInventoryDisplay_Setup_Patch
        {
            public static void Postfix(CM_ArtifactInventoryDisplay __instance)
            {
                //FeatureLogger.Notice($"{nameof(CM_ArtifactInventoryDisplay)} Setup called!");
                GetOrCreateLogDisplayRoot(__instance, out _, isFromStartup: true);
            }
        }

        #endregion ObjectivesScreen
    }
}
