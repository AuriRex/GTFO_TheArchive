using Newtonsoft.Json;
using System.Collections.Generic;

namespace TheArchive.Core.Models
{
    public class RichPresenceSettings
    {
        public bool OverrideSteamRichPresenceText { get; set; } = true;
        public string SteamRPCFormatString { get; set; } = "%Rundown%%Expedition% \"%ExpeditionName%\"";
        public bool UseFormatStringForCopyLobbyIDButton { get; set; } = true;
        public string CopyLobbyIDFormatString { get; set; } = "LF%OpenSlots% %Rundown%%Expedition% \"%ExpeditionName%\": `%LobbyID%`";
        public bool EnableDiscordRichPresence { get; set; } = true;

        public bool DEBUG_EnableRichPresenceLogSpam { get; set; } = false;

        [JsonIgnore]
        private static RichPresenceSettings Default => new RichPresenceSettings();

        public Dictionary<PresenceGameState, GSActivity> DiscordRPCFormat = new Dictionary<PresenceGameState, GSActivity>()
        {
            {
                PresenceGameState.Startup, new GSActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "Initializing ...",
                            Status = "Waking prisoners ...",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "gtfo_icon",
                                LargeTooltip = "GTFO"
                            }
                        }
                    }
                }

            },
            {
                PresenceGameState.NoLobby, new GSActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "Rundown %RundownNumber%: \"%RundownTitle%\"",
                            Status = "Deciding what to do",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "gtfo_icon",
                                LargeTooltip = "GTFO"
                            }
                        }
                    }
                }
            },
            {
                PresenceGameState.InLobby, new GSActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "In Lobby",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "icon_lobby",
                                LargeTooltip = "GTFO %Rundown% \"%RundownTitle%\"",
                                SmallImageKey = "%CharacterImageKey%",
                                SmallTooltip = "Playing as %CharacterName%"
                            },
                            DisplayPartyInfo = true
                        }
                    }
                }
            },
            {
                PresenceGameState.Dropping, new GSActivity()
                {
                    MultiFormatCycleTime = 5,
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "Dropping .",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "icon_dropping",
                                LargeTooltip = "Riding the elevator to hell ..."
                            },
                            DisplayPartyInfo = true
                        },
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "Dropping ..",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "icon_dropping",
                                LargeTooltip = "Riding the elevator to hell ..."
                            },
                            DisplayPartyInfo = true
                        },
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "Dropping ...",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "icon_dropping",
                                LargeTooltip = "Riding the elevator to hell ..."
                            },
                            DisplayPartyInfo = true
                        }
                    }
                }
            },
            {
                PresenceGameState.LevelGenerationFinished, new GSActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "Engaging breaks ...",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "icon_dropping",
                                LargeTooltip = "Next stop: Hell"
                            },
                            DisplayPartyInfo = true
                        }
                    }
                }
            },
            {
                PresenceGameState.InLevel, new GSActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "In Expedition",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "gtfo_icon",
                                LargeTooltip = "Exploring %CurrentZoneShort% Area %AreaSuffix%",
                                SmallImageKey = "%MeleeWeaponKey%",
                                SmallTooltip = "%EquippedMeleeWeaponName%"
                            },
                            DisplayPartyInfo = true
                        },
                        new GSActivityFormat()
                        {
                            Details = "Health: %HealthPercent%%",
                            Status = "In Expedition",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "gtfo_icon",
                                LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                SmallImageKey = "res_meds",
                                SmallTooltip = "Prisoner vitals"
                            },
                            DisplayPartyInfo = true
                        },
                        new GSActivityFormat()
                        {
                            Details = "Primary: %PrimaryAmmoPercent%% Special: %SpecialAmmoPercent%%",
                            Status = "In Expedition",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "gtfo_icon",
                                LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                SmallImageKey = "res_ammo",
                                SmallTooltip = "Ammo levels"
                            },
                            DisplayPartyInfo = true
                        },
                        new GSActivityFormat()
                        {
                            Details = "Tool: %ToolAmmoPercentOrStatus%",
                            Status = "In Expedition",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "%ToolKey%",
                                LargeTooltip = "%EquippedToolName%",
                                SmallImageKey = "res_tool",
                                SmallTooltip = "Resource level"
                            },
                            DisplayPartyInfo = true
                        }
                    }
                }
            },
            {
                PresenceGameState.ExpeditionFailed, new GSActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "EXPD FAILED",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "icon_failed",
                                LargeTooltip = "Beep, Beep, Beeeeeeeeeeep",
                                SmallImageKey = "%CharacterImageKey%",
                                SmallTooltip = "\"%CharacterName%\", Status: DECEASED"
                            },
                            DisplayPartyInfo = true
                        }
                    }
                }
            },
            {
                PresenceGameState.ExpeditionSuccess, new GSActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "EXPD SURVIVED",
                            Assets = new GSActivityAssets
                            {
                                LargeImageKey = "icon_survived",
                                LargeTooltip = "Hydrostasis awaits ...",
                                SmallImageKey = "%CharacterImageKey%",
                                SmallTooltip = "\"%CharacterName%\"\nStatus: ALIVE"
                            },
                            DisplayPartyInfo = true
                        }
                    }
                }
            },
        };

        /// <summary>
        /// Fills all non-existent / null enum values in <see cref="DiscordRPCFormat"/> with the default ones.
        /// </summary>
        /// <returns>Itself</returns>
        public RichPresenceSettings FillDefaultDictValues()
        {
            var other = new RichPresenceSettings();
            foreach(var format in other.DiscordRPCFormat)
            {
                if(!DiscordRPCFormat.TryGetValue(format.Key, out var val) || val == null)
                {
                    DiscordRPCFormat.Add(format.Key, format.Value.FillDefaultDictValues(format.Key));
                }
                else
                {
                    val.FillDefaultDictValues(format.Key);
                }
            }
            return this;
        }

        public class GSActivity
        {
            [JsonIgnore]
            public bool IsMultiFormat => Formats?.Count > 1;
            [JsonIgnore]
            public int Count => Formats?.Count ?? 0;
            [JsonIgnore]
            public int MultiIndex { get; private set; } = 0;
            [JsonIgnore]
            public int CurrentMultiCycleCurrency { get; private set; } = 10;

            public int MultiFormatCycleTime { get; set; } = 10;
            public List<GSActivityFormat> Formats = new List<GSActivityFormat>();

            public GSActivityFormat GetNext()
            {
                if (!IsMultiFormat) return Formats[0];

                if (MultiIndex >= Count) MultiIndex = 0;

                var next = Formats[MultiIndex];

                CurrentMultiCycleCurrency -= 5;

                if (CurrentMultiCycleCurrency <= 0)
                {
                    MultiIndex++;
                    CurrentMultiCycleCurrency = MultiFormatCycleTime;
                }

                return next;
            }

            public GSActivity FillDefaultDictValues(PresenceGameState state)
            {
                var other = RichPresenceSettings.Default;
                if((Formats?.Count ?? 0) == 0)
                {
                    other.DiscordRPCFormat.TryGetValue(state, out var gsaOther);
                    Formats = gsaOther.Formats;
                }
                return this;
            }
        }

        public class GSActivityFormat
        {
            public string Details { get; set; } = "DefaultDetailsFormatString";
            public string Status { get; set; } = "DefaultStatusFormatString";
            public GSActivityAssets Assets { get; set; } = null;
            public bool DisplayTimeElapsed { get; set; } = true;
            public bool DisplayPartyInfo { get; set; } = false;
        }

        public class GSActivityAssets
        {
            public string LargeImageKey { get; set; } = "please_just_work";
            public string LargeTooltip { get; set; } = "Default Large Tooltip";
            public string SmallImageKey { get; set; } = null;
            public string SmallTooltip { get; set; } = null;
        }
    }
}
