using CellMenu;
using GameData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using TMPro;
using UnityEngine;
using static TheArchive.Features.Hud.LogVisualizer;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Hud
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownEight, RundownFlags.Latest)]
    public class LogVisualizer : Feature
    {
        

        public override string Name => "Log Visualizer";

        public override string Group => FeatureGroups.Hud;

        public override string Description => "";


        public override bool ShouldInit()
        {
            return !IsPlayingModded;
        }

        public override void Init()
        {
            LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<CM_LogDisplayRoot>();
            LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<CM_LogDisplayItem>();
        }

        private static eGameStateName _eGameStateName_InLevel = GetEnumFromName<eGameStateName>(nameof(eGameStateName.InLevel));

        public override void OnEnable()
        {
            if (!DataBlocksReady)
                return;

            TryDiscoverAllLogsFromDataBlocks();

            var createdThisFrame = GetOrCreateLogDisplayRoot(MainMenuGuiLayer.Current?.PageObjectives?.m_artifactInventoryDisplay, out var logDisplay);

            if (logDisplay == null)
            {
                FeatureLogger.Warning($"Log Display is null! (This shouldn't happen!)");
            }

            if((eGameStateName)CurrentGameState == _eGameStateName_InLevel)
            {
                // We are in level, update log display
                InitializeLogDisplayForActiveExpedition(logDisplay, createdThisFrame);
            }
        }

        public void OnGameStateChanged(eGameStateName state)
        {
            if(state == _eGameStateName_InLevel)
            {
                var createdThisFrame = GetOrCreateLogDisplayRoot(MainMenuGuiLayer.Current?.PageObjectives?.m_artifactInventoryDisplay, out var logDisplay);
                InitializeLogDisplayForActiveExpedition(logDisplay, createdThisFrame);
            }
        }

        public override void OnDisable()
        {
            DestroyLogDisplayRoot();
            // TODO: Cleanup Log texts on RundownScreen(s)
        }

        private void InitializeLogDisplayForActiveExpedition(CM_LogDisplayRoot logDisplay, bool createdThisFrame)
        {
            var expedition = RundownManager.GetActiveExpeditionData();

            var expIndex = expedition.expeditionIndex;
            var expTier = expedition.tier;
            var rundownKey = expedition.rundownKey.data;

            FeatureLogger.Debug($"GameState == InLevel: Initializing LogDisplay for expedition: {rundownKey} {expTier} {expIndex}");

            var logs = GetLogsForExpedition(rundownKey, expTier, expIndex);

            if (createdThisFrame)
            {
                // Turns out waiting one frame after cloning something fixes weird behaviour with old values/GOs persisting
                LoaderWrapper.StartCoroutine(NextFrame(() =>
                {
                    logDisplay.Initialize(logs);
                }));
            }
            else
            {
                logDisplay.Initialize(logs);
            }
        }

        public static new IArchiveLogger FeatureLogger { get; set; }

        public record class LogToExpedition
        {
            public uint Rundown { get; set; }
            public uint ExpeditionTier { get; set; }
            public uint ExpeditionNumber { get; set; }
            public uint ExpeditionIndex { get; set; }
            public string LogFileName { get; set; }
            public uint LogId { get; set; }
            public bool IsAudioLog { get; set; }
            public eDimensionIndex DimensionIndex { get; internal set; }
            public int Zone { get; internal set; }
            public int ZoneOverride { get; internal set; }

            public override string ToString()
            {
                var rd = RundownDataBlock.GetBlock(Rundown);
                return $"{rd.StorytellingData.Title.ToString().Split('\n')[1].Replace("TITLE: ", string.Empty)}: {char.ConvertFromUtf32(65 + (int)ExpeditionTier)}{ExpeditionNumber + 1}: {LogFileName} | ID: {LogId} | Audio: {IsAudioLog} | {DimensionIndex} | ZONE_{(ZoneOverride >= 0 ? ZoneOverride : Zone)}";
            }
        }

        private static List<LogToExpedition> _allLogs = new List<LogToExpedition>();

        public override void OnDatablocksReady()
        {
            TryDiscoverAllLogsFromDataBlocks();
        }

        private static bool _logsDiscovered = false;
        private void TryDiscoverAllLogsFromDataBlocks()
        {
            if (_logsDiscovered)
                return;

            var setupDB = GameSetupDataBlock.GetAllBlocks()[0];

            foreach (var loadedRundownDBIDs in setupDB.RundownIdsToLoad)
            {
                var rundownDB = RundownDataBlock.GetBlock(loadedRundownDBIDs);

                if (rundownDB == null)
                    return;

                var rundownID = rundownDB.persistentID;

                // iterate tier
                // iterate expedition
                // iterate level layout(s)
                // create LogToExpedition (include dimension info? idk)

                IterateTier(rundownDB.TierA, 0, rundownID);
                IterateTier(rundownDB.TierB, 1, rundownID);
                IterateTier(rundownDB.TierC, 2, rundownID);
                IterateTier(rundownDB.TierD, 3, rundownID);
                IterateTier(rundownDB.TierE, 4, rundownID);

            }

            _allLogs = _allLogs.Distinct().ToList();

            _logsDiscovered = true;

            foreach (var log in _allLogs)
            {
                FeatureLogger.Info($"{log}");
            }

            FeatureLogger.Info("-----");
            FeatureLogger.Success($"{_allLogs.Count} total log files");


            FeatureLogger.Info("-----");
        }

        private static void IterateTier(Il2CppSystem.Collections.Generic.List<ExpeditionInTierData> tiers, uint tierthing, uint rundown)
        {
            if (tiers == null)
                return;

            uint c = 0;
            uint i = 0;
            foreach(var expedition in tiers)
            {
                if (expedition == null)
                    return;

                IterateLayer(expedition.LevelLayoutData, tierthing, rundown, c, eDimensionIndex.Reality, i);
                IterateLayer(expedition.SecondaryLayout, tierthing, rundown, c, eDimensionIndex.Reality, i);
                IterateLayer(expedition.ThirdLayout, tierthing, rundown, c, eDimensionIndex.Reality, i);

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

                                var info = new LogToExpedition
                                {
                                    Rundown = rundown,
                                    ExpeditionTier = tierthing,
                                    ExpeditionNumber = c,
                                    ExpeditionIndex = i,
                                    LogFileName = logFile.FileName,
                                    LogId = logFile.FileContent.Id,
                                    IsAudioLog = logFile.AttachedAudioFile != 0,
                                    DimensionIndex = dimensionIndex,
                                    Zone = 0,
                                    ZoneOverride = 0,
                                };
                                _allLogs.Add(info);
                            }
                        }
                    }

                    IterateLayer(dimensionDB.DimensionData.LevelLayoutData, tierthing, rundown, c, dimensionIndex, i);
                }

                if(expedition.Accessibility != eExpeditionAccessibility.AlwayBlock)
                    c++;
                i++;
            }
        }

        private static void IterateLayer(uint layoutDataId, uint tier, uint rundown, uint expeditionNum, eDimensionIndex dimensionIndex, uint expeditionIndex)
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
                        if (term.LocalLogFiles == null)
                            continue;

                        TerminalToLogs(tier, rundown, expeditionNum, dimensionIndex, level, zone, term, expeditionIndex);
                    }
                }
                
                if(zone.TerminalPlacements != null)
                {
                    foreach (var term in zone.TerminalPlacements)
                    {
                        if (term.LocalLogFiles == null)
                            continue;

                        TerminalToLogs(tier, rundown, expeditionNum, dimensionIndex, level, zone, term, expeditionIndex);
                    }
                }
                
            }
        }

        private static void TerminalToLogs(uint tier, uint rundown, uint expeditionNum, eDimensionIndex dimensionIndex, LevelLayoutDataBlock level, ExpeditionZoneData zone, TerminalPlacementData term, uint expeditionIndex)
        {
            foreach (var logFile in term.LocalLogFiles)
            {
                if (logFile.FileContent.Id == 0)
                    continue;

                var info = new LogToExpedition
                {
                    Rundown = rundown,
                    ExpeditionTier = tier,
                    ExpeditionNumber = expeditionNum,
                    ExpeditionIndex = expeditionIndex,
                    LogFileName = logFile.FileName,
                    LogId = logFile.FileContent.Id,
                    IsAudioLog = logFile.AttachedAudioFile != 0,
                    DimensionIndex = dimensionIndex,
                    Zone = zone.Alias == 0 ? level.ZoneAliasStart + (int)zone.LocalIndex : zone.Alias,
                    ZoneOverride = zone.AliasOverride,
                };

                _allLogs.Add(info);
            }
        }

        public static Achievement_ReadAllLogs GetReadAllLogsInstance()
        {
            if (AchievementManager.Current?.m_allAchievements == null)
                return null;

            foreach(var ach in AchievementManager.Current.m_allAchievements)
            {
                if (ach.GetIl2CppType().Name == nameof(Achievement_ReadAllLogs))
                    return ach.TryCastTo<Achievement_ReadAllLogs>();
            }

            return null;
        }

        private static readonly ISet<LogEntry> _emptyLogSet = new HashSet<LogEntry>();
        public ISet<LogEntry> GetLogsForExpedition(string rundownKey, eRundownTier eExpTier, int expIndex)
        {
#warning TODO
            //throw new NotImplementedException();

            var expTier = (int)eExpTier - 1;

            if (!int.TryParse(rundownKey.Split('_')[1], out var rundownId))
            {
                FeatureLogger.Warning($"Failed to parse Rundown ID from \"{rundownKey}\" (Tier: {expTier}, Index: {expIndex})");
                return _emptyLogSet;
            }

            var ral = GetReadAllLogsInstance();

            if (ral == null || ral.m_allLogs == null || ral.m_readLogsOnStart == null)
                return _emptyLogSet;

            var logsForExpedition = _allLogs.Where(log => log.Rundown == rundownId && log.ExpeditionTier == expTier && log.ExpeditionIndex == expIndex && ral.m_allLogs.Contains(log.LogId));
            var logsIHave = _allLogs.Where(log => log.Rundown == rundownId && log.ExpeditionTier == expTier && log.ExpeditionIndex == expIndex && ral.m_readLogsOnStart.Contains(log.LogId));

            var logsForExpeditionDistinct = logsForExpedition.Select(log => log.LogId).Distinct().ToArray();
            var logsIHaveDistinct = logsIHave.Select(log => log.LogId).Distinct().ToArray();

            var set = new HashSet<LogEntry>();
            foreach(var id in logsForExpeditionDistinct)
            {
                var log = logsForExpedition.FirstOrDefault(log => log.LogId == id);

                if (log == null)
                    continue;

                set.Add(new LogEntry(id, log.LogFileName, logsIHaveDistinct.Any(lih => lih == id), log));
            }

            return set;
        }

        [ArchivePatch(typeof(Achievement_ReadAllLogs), nameof(Achievement_ReadAllLogs.OnPlayFabLoginSuccess))]
        internal static class Achievement_ReadAllLogs_OnPlayFabLoginSuccess_Patch
        {
            public static void Postfix(Achievement_ReadAllLogs __instance)
            {
                /*foreach(var log in __instance.m_readLogsOnStart)
                {
                    
                }*/

                //
                var wtfIsGoingON = _allLogs.Where(log => !__instance.m_allLogs.Contains(log.LogId)).ToList();

                var allLogs = _allLogs.Where(log => __instance.m_allLogs.Contains(log.LogId)).ToList();
                var allLogsDistinct = _allLogs.Select(log => log.LogId).Distinct().ToArray();

                var logsIhave = allLogs.Where(log => __instance.m_readLogsOnStart.Contains(log.LogId));
                var logsIHaveDistinct = logsIhave.Select(log => log.LogId).Distinct().ToArray();

                FeatureLogger.Notice($"{logsIHaveDistinct.Length}/{allLogsDistinct.Length} Logs found!");

                foreach(var log in logsIhave)
                {
                    FeatureLogger.Info($"{log}");
                }
                FeatureLogger.Success("---");

                FeatureLogger.Fail("Missing ones?:");
                foreach(var logInfo in wtfIsGoingON)
                {
                    FeatureLogger.Success(logInfo.ToString());
                }

                var abcdefg = __instance.m_allLogs.ToSystemList().Where(id => !_allLogs.Any(log => log.LogId == id));

                foreach(var logId in abcdefg)
                {
                    FeatureLogger.Notice($"Missing Log: {logId}");
                }
            }
        }

        private static readonly string[] _br = new string[] { "<br>" };

        //UpdateRundownSelectionButton(CM_RundownSelection rundownSelection, System.UInt32 rundownId)
        [ArchivePatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.UpdateRundownSelectionButton))]
        internal static class CM_PageRundown_New_UpdateRundownSelectionButton_Patch
        {
            public static readonly Vector3 _og_friendsHostingTextPos = new Vector3(0, 16.4f, 0);
            public static readonly Vector3 _new_friendsHostingTextPos = new Vector3(0, 76.4f, 0); // Moves it above

            public static void Postfix(CM_RundownSelection rundownSelection, int rundownId)
            {
                //FeatureLogger.Debug($"Updating Rundown Selection Logs Text for Rundown ID \"{rundownId}\" ...");

                var text = rundownSelection.m_rundownText;

                if (text == null)
                    return;

                var rundownText = text.text.Contains("<br>") ? text.text.Split(_br, StringSplitOptions.None)[0] : text.text;

                var ral = GetReadAllLogsInstance();

                if (ral == null || ral.m_allLogs == null || ral.m_readLogsOnStart == null)
                    return;

                var logsForRundown = _allLogs.Where(log => log.Rundown == rundownId && ral.m_allLogs.Contains(log.LogId));
                var logsIHave = _allLogs.Where(log => log.Rundown == rundownId && ral.m_readLogsOnStart.Contains(log.LogId));

                var logsForRundownDistinct = logsForRundown.Select(log => log.LogId).Distinct().ToArray();
                var logsIHaveDistinct = logsIHave.Select(log => log.LogId).Distinct().ToArray();


                var logsText = $"<br><size=60%>Logs: ({logsIHaveDistinct.Length} / {logsForRundownDistinct.Length})</size>";

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

                var originalStatus = __instance.m_statusText.text.Contains("<br>") ? __instance.m_statusText.text.Split(_br, StringSplitOptions.None)[0] : __instance.m_statusText.text;

                var ral = GetReadAllLogsInstance();

                if (ral == null || ral.m_allLogs == null || ral.m_readLogsOnStart == null)
                    return;

                var expIndex = __instance.ExpIndex;
                var expTier = (int)__instance.Tier - 1;

                if(!int.TryParse(__instance.RundownKey.Split('_')[1], out var rundownId))
                {
                    FeatureLogger.Warning($"Failed to parse Rundown ID from \"{__instance.RundownKey}\" ({__instance.FullName})");
                    return;
                }

                __instance.m_hostingFriendsCountText.transform.localPosition = _new_friendsHostingTextPos;

                var logsForExpedition = _allLogs.Where(log => log.Rundown == rundownId && log.ExpeditionTier == expTier && log.ExpeditionIndex == expIndex && ral.m_allLogs.Contains(log.LogId));
                var logsIHave = _allLogs.Where(log => log.Rundown == rundownId && log.ExpeditionTier == expTier && log.ExpeditionIndex == expIndex && ral.m_readLogsOnStart.Contains(log.LogId));

                var logsForExpeditionDistinct = logsForExpedition.Select(log => log.LogId).Distinct().ToArray();
                var logsIHaveDistinct = logsIHave.Select(log => log.LogId).Distinct().ToArray();

                string logsText = $"{logsIHaveDistinct.Length} / {logsForExpeditionDistinct.Length}";
                string prefixColor = "<#FFF>";

                if(logsForExpeditionDistinct.Length == 0)
                {
                    logsText = "N/A";
                    prefixColor = "<#777>";
                }
                else if(logsIHaveDistinct.Length == logsForExpeditionDistinct.Length)
                {
                    prefixColor = "<color=orange>";
                }

                __instance.m_statusText.SetText($"{originalStatus}<br>{prefixColor}Logs: ({logsText})</color>");
            }
        }

        #region ObjectivesScreen

        public class LogEntry
        {
            public readonly uint logId;
            public readonly string logName;
            public readonly bool collected;
            public readonly LogToExpedition details;

            public LogEntry(uint id, string logFileName, bool collected, LogToExpedition details)
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

            public static readonly string NoLogsAvailableText = "Logs N/A";

            public Vector3 LocalItemRootPos { get; internal set; }
            public static Vector3 ItemDistance { get; private set; } = new Vector3(0, -45, 0);

            public TextMeshPro HeaderText { get; internal set; }
            //public static GameObject ItemPrefab { get; internal set; }

            public static string PrefabGOName { get; internal set; }

            private List<CM_LogDisplayItem> _items = new List<CM_LogDisplayItem>();

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

            public void Initialize(ISet<LogEntry> logs)
            {
                Cleanup();

                if(logs.Count > 0)
                {
                    foreach (var entry in logs)
                    {
                        CreateItem(entry.logId, entry.logName, entry.collected);
                    }
                }

                UpdateHeaderText();
            }

            private void UpdateHeaderText()
            {
                if (_items.Count == 0)
                {
                    HeaderText.SetText(NoLogsAvailableText);
                    return;
                }

                var collectedAmount = _items.Where(item => item.Collected).Count();

                HeaderText.SetText($"Logs: ({collectedAmount} / {_items.Count})");
            }

            private void CreateItem(uint logid, string logName, bool collected)
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

                logDisplayItem.SetCollected(collected);

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

            public static string LogUnknownText { get; private set; } = "???";

            public TextMeshPro Text;
            public SpriteRenderer Divider;
            public SpriteRenderer Gradient;

            public bool Collected { get; private set; }
            public uint LogID { get; internal set; } = 0;
            public string LogName { get; internal set; } = string.Empty;

            internal void SetCollected(bool collected)
            {
                Collected = collected;

                if (collected)
                {
                    Text.SetText(LogName);
                    Text.color = Color.white;
                    Divider.color = Color.green;
                    Gradient.color = Color.green.WithAlpha(0.025f);
                    return;
                }

                Text.SetText(LogUnknownText);
                Text.color = Color.gray;
                Divider.color = Color.red;
                Gradient.color = Color.red.WithAlpha(0.025f);
            }
        }

        private static CM_LogDisplayRoot _logDisplayRoot = null;

        private void DestroyLogDisplayRoot()
        {
            _logDisplayRoot.gameObject.SetActive(false);

            /*_logDisplayRoot.SafeDestroyGO();
            _logDisplayRoot = null;*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artifactInvDisplay"></param>
        /// <param name="logDisplayRoot"></param>
        /// <param name="isFromStartup"></param>
        /// <returns>If the object was created this frame</returns>
        public static bool GetOrCreateLogDisplayRoot(CM_ArtifactInventoryDisplay artifactInvDisplay, out CM_LogDisplayRoot logDisplayRoot, bool isFromStartup = false)
        {
            if (artifactInvDisplay == null || _logDisplayRoot != null)
            {
                _logDisplayRoot?.gameObject?.SetActive(true);
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
            headerText.SetText("Logs");

            logDisplayRoot.HeaderText = headerText;
            logDisplayRoot.LocalItemRootPos = logItemPrefab.transform.localPosition;
            CM_LogDisplayRoot.PrefabGOName = logItemPrefab.gameObject.name;

            _logDisplayRoot = logDisplayRoot;

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
                FeatureLogger.Notice($"{nameof(CM_ArtifactInventoryDisplay)} Setup called!");
                GetOrCreateLogDisplayRoot(__instance, out _, isFromStartup: true);
            }
        }

        #endregion ObjectivesScreen
    }
}
