using TheArchive.Utilities;

namespace TheArchive.Models.Progression
{
    public struct ExpeditionCompletionData
    {
        public string RundownIdString => RawSessionData.RundownId;
        private uint _rundownId { get; set; }
        public uint RundownId
        {
            get
            {
                if(_rundownId == 0)
                {
                    if (!uint.TryParse(RundownIdString.Replace("Local_", string.Empty), out var rundownId))
                    {
                        ArchiveLogger.Error($"[{nameof(ExpeditionCompletionData)}] Could not parse rundown id \"{RundownIdString}\"!");
                        return 0;
                    }
                }
                return _rundownId;
            }
        }
        public string ExpeditionId => RawSessionData.ExpeditionId;
        public string SessionId => RawSessionData.SessionId;
        public int ArtifactsCollected => RawSessionData.ArtifactsCollected;
        public bool WasPrisonerEfficiencyClear => RawSessionData.PrisonerEfficiencyCompleted;
        public bool WasFirstTimeCompletion { get; internal set; }
        public ExpeditionSession RawSessionData { get; internal set; }
        /// <summary>
        /// Artifact Heat before this session
        /// </summary>
        public float PreArtifactHeat { get; internal set; }
        /// <summary>
        /// Artifact Heat after session completion
        /// </summary>
        public float NewArtifactHeat { get; internal set; }
    }
}
