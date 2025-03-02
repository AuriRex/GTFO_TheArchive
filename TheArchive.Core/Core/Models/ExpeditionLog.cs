using System;
using System.Collections.Generic;

namespace TheArchive.Core.Models;

public class ExpeditionLog
{
    public List<ExpeditionLogEntry> Entries { get; set; } = new List<ExpeditionLogEntry>();

    public class ExpeditionLogEntry
    {
        public Utilities.Utils.RundownID Rundown { get; set; }
        public char ExpeditionTier { get; set; }
        public int ExpeditionIndex { get; set; }

        [JsonIgnore]
        public DateTimeOffset TimestampDateTimeOffset
        {
            get => DateTimeOffset.FromUnixTimeSeconds(Timestamp);
            set => Timestamp = value.ToUnixTimeSeconds();
        }
        public long Timestamp { get; set; }

        public bool Successful { get; set; }
        public bool CheckPointUsed { get; set; } = false;
        public ExpeditionLogPartyInfo Party { get; set; }
    }

    public class ExpeditionLogPartyInfo
    {
        /// <summary> SteamIDs of the players in the current expedition </summary>
        List<uint> Players { get; set; } = new List<uint>();
    }
}