using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace TheArchive.Features.Dev
{
    //[EnableFeatureByDefault]
    internal class UpdateChecker : Feature
    {
        public override string Name => "Update Checker";

        public override string Group => FeatureGroups.Dev;

        public override string Description => "Checks if a new version of the mod is available via github.";


        public new static IArchiveLogger FeatureLogger { get; set; }


        public static bool HasReleaseInfo => LatestReleaseInfo.HasValue;
        public static ReleaseInfo? LatestReleaseInfo { get; private set; } = null;
        public static bool IsOnLatestRelease => LatestReleaseInfo?.Tag == ArchiveMod.GIT_BASE_TAG;

        private static HttpClient _client;
        private static readonly Stopwatch _stopwatch = new Stopwatch();
        private static DateTimeOffset _startTime;

        public override void Init()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            _client.DefaultRequestHeaders.Add("User-Agent", "TheArchive GTFO Mod Update Checker");
        }

        private static Task<ReleaseInfo> _updaterTask;

        public record struct ReleaseInfo(string Tag, string Name, string PublishedAt, string URL, string ReleaseNotes);

        public override void OnEnable()
        {
            /*if (ArchiveMod.GIT_IS_DIRTY)
            {
                return;
            }*/

            if (_updaterTask != null && !_updaterTask.IsCompleted && !_updaterTask.IsFaulted && !_updaterTask.IsCanceled)
            {
                // Already running
                return;
            }

            _stopwatch.Reset();
            _stopwatch.Start();

            _updaterTask = Task.Run(GetLatestReleaseInfo);
        }

        public override void Update()
        {
            if(_updaterTask == null || _updaterTask.IsCanceled || _updaterTask.IsFaulted)
            {
                FeatureLogger.Error("Update checker failed!");
                FeatureManager.Instance.DisableFeature(this, setConfig: false);
                return;
            }

            if (!_updaterTask.IsCompleted)
                return;

            var releaseInfo = _updaterTask.Result;
            LatestReleaseInfo = releaseInfo;

            FeatureLogger.Debug($"Took {_stopwatch.Elapsed}");

            var currentTag = ArchiveMod.GIT_BASE_TAG; // "v0.5.36-beta"; // 
            // Highly advanced version checking & comparing algorithm right here:
            if (releaseInfo.Tag != currentTag)
            {
                FeatureLogger.Notice($"Version mismatch, an update might be available! Installed:{currentTag}, Online: {releaseInfo.Tag}");
            }
            else
            {
                FeatureLogger.Success($"Running latest available mod version: {releaseInfo.Tag}");
            }

            FeatureManager.Instance.DisableFeature(this, setConfig: false);
        }

        private static async Task<ReleaseInfo> GetLatestReleaseInfo()
        {
            var json = await GetLatestRelease();

            dynamic response = ParseJson(json);

            var tag = $"{response.tag_name}";
            var name = $"{response.name}";
            var publishedAt = $"{response.published_at}";
            var url = $"{response.html_url}";
            var releaseNotes = $"{response.body}";

            _stopwatch.Stop();

            return new ReleaseInfo(tag, name, publishedAt, url, releaseNotes);
        }

        private static async Task<string> GetLatestRelease()
        {
            return await _client.GetStringAsync($"https://api.github.com/repos/{ArchiveMod.GITHUB_OWNER_NAME}/{ArchiveMod.GITHUB_REPOSITORY_NAME}/releases/latest");
        }

        private static dynamic ParseJson(string json)
        {
            return JObject.Parse(json);
        }

    }
}
