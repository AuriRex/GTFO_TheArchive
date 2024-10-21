using System.Collections.Generic;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.Models;

namespace TheArchive.Core.Settings
{
    public class RichPresenceSettings
    {
        [FSDisplayName("Disable on R8")]
        [FSDescription("Only disables Custom Discord Rich Presence on the Rundown 8 game version.\n\nIt will still be enabled if playing on older game versions.")]
        [FSRundownHint(Utilities.Utils.RundownFlags.RundownEight, Utilities.Utils.RundownFlags.Latest)]
        public bool DisableOnRundownEight { get; set; } = true;

        [FSHide]
        [FSDisplayName("DEBUG Use Default Settings")]
        public bool DEBUG_UseDefaultSettings { get; set; } = false;

        [FSIgnore]
        public bool DEBUG_RichPresenceLogSpam { get; set; } = false;

        [JsonIgnore, FSIgnore]
        internal static RichPresenceSettings Default => new RichPresenceSettings();

        public Dictionary<PresenceGameState, GSTopActivity> DiscordRPCFormat = new Dictionary<PresenceGameState, GSTopActivity>()
        {
            {
                PresenceGameState.Startup, new GSTopActivity()
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
                PresenceGameState.NoLobby, new GSTopActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "%RundownWithNumberOrModdedPrefix%: \"%RundownTitle%\"",
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
                PresenceGameState.InLobby, new GSTopActivity()
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
                PresenceGameState.Dropping, new GSTopActivity()
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
                PresenceGameState.LevelGenerationFinished, new GSTopActivity()
                {
                    Formats = new List<GSActivityFormat>()
                    {
                        new GSActivityFormat()
                        {
                            Details = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                            Status = "Engaging brakes ...",
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
                PresenceGameState.InLevel, new GSTopActivity()
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
                    },
                    SubActivities = new List<GSSubActivity>()
                    {
                        new GSSubActivity()
                        {
                            // Reactor StartUP Sequence Intro Normal
                            DisplayConditions = new List<string>()
                            {
                                "%IsReactorActive%",
                                "%IsReactorInIntro%",
                                "%IsReactorTypeStartup%",
                                "!%IsReactorInVerifyFailState%"
                            },
                            Formats = new List<GSActivityFormat>()
                            {
                                new GSActivityFormat()
                                {
                                    Details = "Reactor %ReactorType% (%ReactorWaveCountCurrent%/%ReactorWaveCountMax%)",
                                    Status = "Warming Up!",
                                    Assets = new GSActivityAssets
                                    {
                                        LargeImageKey = DRPIcons.Expedition.Reactor,
                                        LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                    },
                                    DisplayPartyInfo = true,
                                    DisplayStateTimeElapsed = false,
                                    CustomTimeProvider = "%ReactorWaveEndTime%",
                                }
                            }
                        },
                        new GSSubActivity()
                        {
                            // Reactor StartUP Sequence Intro Failed Verification
                            DisplayConditions = new List<string>()
                            {
                                "%IsReactorActive%",
                                "%IsReactorInIntro%",
                                "%IsReactorTypeStartup%",
                                "%IsReactorInVerifyFailState%"
                            },
                            Formats = new List<GSActivityFormat>()
                            {
                                new GSActivityFormat()
                                {
                                    Details = "%ReactorType% Sequence Failed! (%ReactorWaveCountCurrent%/%ReactorWaveCountMax%)",
                                    Status = "Warming Up!",
                                    Assets = new GSActivityAssets
                                    {
                                        LargeImageKey = DRPIcons.Expedition.Reactor,
                                        LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                    },
                                    DisplayPartyInfo = true,
                                    DisplayStateTimeElapsed = false,
                                    CustomTimeProvider = "%ReactorWaveEndTime%",
                                }
                            }
                        },
                        new GSSubActivity()
                        {
                            // Reactor ShutDOWN Sequence Intro
                            DisplayConditions = new List<string>()
                            {
                                "%IsReactorActive%",
                                "%IsReactorInIntro%",
                                "!%IsReactorTypeStartup%"
                            },
                            Formats = new List<GSActivityFormat>()
                            {
                                new GSActivityFormat()
                                {
                                    Details = "Reactor %ReactorType% Sequence",
                                    Status = "Warning!",
                                    Assets = new GSActivityAssets
                                    {
                                        LargeImageKey = DRPIcons.Expedition.Reactor,
                                        LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                    },
                                    DisplayPartyInfo = true,
                                }
                            }
                        },
                        new GSSubActivity()
                        {
                            // Reactor StartUP Wave
                            DisplayConditions = new List<string>()
                            {
                                "%IsReactorActive%",
                                "%IsReactorWaveOrChaosActive%",
                                "%IsReactorTypeStartup%"
                            },
                            Formats = new List<GSActivityFormat>()
                            {
                                new GSActivityFormat()
                                {
                                    Details = "Reactor Wave (%ReactorWaveCountCurrent%/%ReactorWaveCountMax%)",
                                    Status = "High Intensive Test",
                                    Assets = new GSActivityAssets
                                    {
                                        LargeImageKey = DRPIcons.Expedition.Reactor,
                                        LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                        SmallImageKey = DRPIcons.EnemyPing,
                                        SmallTooltip = "Heavy Reactor Load"
                                    },
                                    DisplayPartyInfo = true,
                                    DisplayStateTimeElapsed = false,
                                    CustomTimeProvider = "%ReactorWaveEndTime%",
                                }
                            }
                        },
                        new GSSubActivity()
                        {
                            // Reactor ShutDOWN Puzzle / Chaos
                            DisplayConditions = new List<string>()
                            {
                                "%IsReactorActive%",
                                "%IsReactorWaveOrChaosActive%",
                                "!%IsReactorTypeStartup%"
                            },
                            Formats = new List<GSActivityFormat>()
                            {
                                new GSActivityFormat()
                                {
                                    Details = "Reactor Shutdown Failure!",
                                    Status = "Alarm triggered!",
                                    Assets = new GSActivityAssets
                                    {
                                        LargeImageKey = DRPIcons.Expedition.Reactor,
                                        LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                        SmallImageKey = DRPIcons.EnemyPing,
                                        SmallTooltip = "Security System Malfunctioning"
                                    },
                                    DisplayPartyInfo = true,
                                }
                            }
                        },
                        new GSSubActivity()
                        {
                            // Reactor StartUP Sequence Awaiting Verify
                            DisplayConditions = new List<string>()
                            {
                                "%IsReactorActive%",
                                "%IsReactorAwaitingVerify%",
                                "%IsReactorTypeStartup%"
                            },
                            Formats = new List<GSActivityFormat>()
                            {
                                new GSActivityFormat()
                                {
                                    Details = "%ReactorVerificationString% (%ReactorWaveCountCurrent%/%ReactorWaveCountMax%)",
                                    Status = "Verification Required!",
                                    Assets = new GSActivityAssets
                                    {
                                        LargeImageKey = DRPIcons.Expedition.Reactor,
                                        LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                    },
                                    DisplayPartyInfo = true,
                                    DisplayStateTimeElapsed = false,
                                    CustomTimeProvider = "%ReactorWaveEndTime%",
                                }
                            }
                        },
                        new GSSubActivity()
                        {
                            // Reactor ShutDOWN Sequence Awaiting Verify
                            DisplayConditions = new List<string>()
                            {
                                "%IsReactorActive%",
                                "%IsReactorAwaitingVerify%",
                                "!%IsReactorTypeStartup%"
                            },
                            Formats = new List<GSActivityFormat>()
                            {
                                new GSActivityFormat()
                                {
                                    Details = "Initiating Shutdown Sequence ...",
                                    Status = "Verification Required!",
                                    Assets = new GSActivityAssets
                                    {
                                        LargeImageKey = DRPIcons.Expedition.Reactor,
                                        LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                    },
                                    DisplayPartyInfo = true,
                                }
                            }
                        },
                        new GSSubActivity()
                        {
                            // Reactor Sequence Completed
                            DisplayConditions = new List<string>()
                            {
                                "%IsReactorActive%",
                                "%IsReactorCompleted%"
                            },
                            Formats = new List<GSActivityFormat>()
                            {
                                new GSActivityFormat()
                                {
                                    Details = "Reactor %ReactorType% Sequence",
                                    Status = "Complete!",
                                    Assets = new GSActivityAssets
                                    {
                                        LargeImageKey = DRPIcons.Expedition.Reactor,
                                        LargeTooltip = "%Rundown% %Expedition% \"%ExpeditionName%\"",
                                    },
                                    DisplayPartyInfo = true,
                                }
                            }
                        }
                    }
                }
            },
            {
                PresenceGameState.ExpeditionFailed, new GSTopActivity()
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
                PresenceGameState.ExpeditionSuccess, new GSTopActivity()
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
                                SmallTooltip = "\"%CharacterName%\", Status: ALIVE"
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

        public class GSTopActivity : GSActivity
        {
            [JsonIgnore]
            public bool HasSubActivities => (SubActivities?.Count ?? 0) > 0;

            public List<GSSubActivity> SubActivities = new List<GSSubActivity>();

            public GSTopActivity FillDefaultDictValues(PresenceGameState state)
            {
                var defaultSettings = RichPresenceSettings.Default;
                if (!this.HasActivities)
                {
                    defaultSettings.DiscordRPCFormat.TryGetValue(state, out var defaultActivity);
                    Formats = defaultActivity.Formats;

                    if(!this.HasSubActivities && defaultActivity.HasSubActivities)
                    {
                        foreach(var subAct in defaultActivity.SubActivities)
                        {
                            SubActivities.Add(subAct);
                        }
                    }
                }
                return this;
            }
        }

        public class GSSubActivity : GSActivity
        {
            /// <summary>
            /// Bool variables to check for sub activity entry
            /// </summary>
            public List<string> DisplayConditions = new List<string>();
            public bool DisplayConditionsAnyMode = false;
        }

        public class GSActivity
        {
            [JsonIgnore]
            public bool HasActivities => Formats?.Count > 0;
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
        }

        public class GSActivityFormat
        {
            public string Details { get; set; } = "DefaultDetailsFormatString";
            public string Status { get; set; } = "DefaultStatusFormatString";
            public GSActivityAssets Assets { get; set; } = null;
            public bool DisplayStateTimeElapsed { get; set; } = true;
            public string CustomTimeProvider { get; set; } = "";
            public bool CustomProviderIsEndTime { get; set; } = true;
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
