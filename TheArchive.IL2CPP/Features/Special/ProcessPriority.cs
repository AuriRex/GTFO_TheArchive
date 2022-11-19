﻿using System.Collections.Generic;
using System.Diagnostics;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;

namespace TheArchive.Features.Special
{
    [EnableFeatureByDefault]
    public class ProcessPriority : Feature
    {
        public override string Name => "Process Priority";

        public override string Group => FeatureGroups.Special;

        public override string Description => $"Set the games process priority.\n\nThis does the same thing as opening up Taskmanager, going into the 'Details' tab and right clicking on GTFO.exe > [Set Priority]\n\nWarning! Your system might lag / stutter while the game is loading if set to {ProcessPrioritySettings.PriorityClass.AboveNormal} or higher!";

        public class ProcessPrioritySettings
        {
            public PriorityClass Priority { get; set; } = PriorityClass.AboveNormal;

            public enum PriorityClass
            {
                High,
                AboveNormal,
                Normal,
                BelowNormal,
            }
        }

        private static readonly Dictionary<ProcessPrioritySettings.PriorityClass, ProcessPriorityClass> _prioMap = new Dictionary<ProcessPrioritySettings.PriorityClass, ProcessPriorityClass>()
        {
            { ProcessPrioritySettings.PriorityClass.High, ProcessPriorityClass.High },
            { ProcessPrioritySettings.PriorityClass.AboveNormal, ProcessPriorityClass.AboveNormal },
            { ProcessPrioritySettings.PriorityClass.Normal, ProcessPriorityClass.Normal },
            { ProcessPrioritySettings.PriorityClass.BelowNormal, ProcessPriorityClass.BelowNormal },
        };

        [FeatureConfig]
        public static ProcessPrioritySettings Settings { get; set; }

        public override void OnEnable()
        {
            SetPriority(Settings.Priority);
        }

        public override void OnDisable()
        {
            SetPriority(ProcessPrioritySettings.PriorityClass.Normal);
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            SetPriority(Settings.Priority);
        }

        public void SetPriority(ProcessPrioritySettings.PriorityClass settingsPriority)
        {
            if (!_prioMap.TryGetValue(settingsPriority, out var priority))
                return;

            using (Process process = Process.GetCurrentProcess())
                process.PriorityClass = priority;

            FeatureLogger.Success($"Set ProcessPriority to {settingsPriority}!");
        }
    }
}
