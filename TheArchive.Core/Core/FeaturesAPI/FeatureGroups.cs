using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Localization;

namespace TheArchive.Core.FeaturesAPI
{
    public static class FeatureGroups
    {
        #region Setup
        static FeatureGroups()
        {
            Accessibility.SetLanguage(Language.English, "Accessibility");
            Accessibility.SetLanguage(Language.Chinese, "辅助功能");

            ArchiveCore.SetLanguage(Language.English, "Archive Core");
            ArchiveCore.SetLanguage(Language.Chinese, "核心");

            Audio.SetLanguage(Language.English, "Audio");
            Audio.SetLanguage(Language.Chinese, "音频");

            Backport.SetLanguage(Language.English, "Backport");
            Backport.SetLanguage(Language.Chinese, "反向移植");

            Cosmetic.SetLanguage(Language.English, "Cosmetic");
            Cosmetic.SetLanguage(Language.Chinese, "美化");

            Dev.SetLanguage(Language.English, "Developer");
            Dev.SetLanguage(Language.Chinese, "开发者");

            Fixes.SetLanguage(Language.English, "Fixes");
            Fixes.SetLanguage(Language.Chinese, "修复");

            Hud.SetLanguage(Language.English, "HUD / UI");
            Hud.SetLanguage(Language.Chinese, "界面");

            LocalProgression.SetLanguage(Language.English, "Local Progression");
            LocalProgression.SetLanguage(Language.Chinese, "本地进度");

            Special.SetLanguage(Language.English, "Misc");
            Special.SetLanguage(Language.Chinese, "杂项");

            Presence.SetLanguage(Language.English, "Discord / Steam Presence");
            Presence.SetLanguage(Language.Chinese, "Discord / Steam 在线状态");

            Security.SetLanguage(Language.English, "Security / Anti Cheat");
            Security.SetLanguage(Language.Chinese, "安全 / 反作弊");

            QualityOfLife.SetLanguage(Language.English, "Quality of Life");
            QualityOfLife.SetLanguage(Language.Chinese, "生活质量");
        }
        #endregion


        private static FeatureGroup GetOrCreateArchiveGroup(string name, bool dev = false)
        {
            var group = ArchiveCoreGroups.FirstOrDefault(g => g.Name == name, null);

            if (group != null)
                group.IsNewlyCreated = false;

            group ??= new FeatureGroup(name, false, null, dev);

            ArchiveCoreGroups.Add(group);

            return group;
        }


        public static FeatureGroup GetOrCreateModuleGroup(string name, Dictionary<Language, string> groupLanguages = null)
        {
            var group = ModuleGroups.FirstOrDefault(g => g.Name == name, null);

            if (group != null)
                group.IsNewlyCreated = false;

            group ??= new FeatureGroup(name, true, null);

            if (groupLanguages != null)
                group.SetLanguage(groupLanguages);

            return group;
        }

        internal static FeatureGroup GetGroup(string name) => AllGroups.FirstOrDefault(g => g.Name == name);
        internal static FeatureGroup GetModuleGroup(string name) => ModuleGroups.FirstOrDefault(g => g.Name == name);
        internal static HashSet<FeatureGroup> ArchiveCoreGroups { get; private set; } = new();
        internal static HashSet<FeatureGroup> ModuleGroups { get; private set; } = new();
        internal static HashSet<FeatureGroup> AllGroups { get; private set; } = new();

        #region Archive Groups
        internal static FeatureGroup ArchiveCore { get; private set; } = GetOrCreateArchiveGroup(ArchiveMod.ARCHIVE_CORE_FEATUREGROUP);
        public static FeatureGroup Accessibility { get; private set; } = GetOrCreateArchiveGroup("Accessibility");
        public static FeatureGroup Audio { get; private set; } = GetOrCreateArchiveGroup("Audio");
        public static FeatureGroup Backport { get; private set; } = GetOrCreateArchiveGroup("Backport");
        public static FeatureGroup Cosmetic { get; private set; } = GetOrCreateArchiveGroup("Cosmetic");
        public static FeatureGroup Dev { get; private set; } = GetOrCreateArchiveGroup("Developer", true);
        public static FeatureGroup Fixes { get; private set; } = GetOrCreateArchiveGroup("Fixes");
        public static FeatureGroup Hud { get; private set; } = GetOrCreateArchiveGroup("HUD / UI");
        public static FeatureGroup LocalProgression { get; private set; } = GetOrCreateArchiveGroup("Local Progression"); // , group => group.InlineSettings = true
        public static FeatureGroup Special { get; private set; } = GetOrCreateArchiveGroup("Misc");
        public static FeatureGroup Presence { get; private set; } = GetOrCreateArchiveGroup("Discord / Steam Presence");
        public static FeatureGroup Security { get; private set; } = GetOrCreateArchiveGroup("Security / Anti Cheat");
        public static FeatureGroup QualityOfLife { get; private set; } = GetOrCreateArchiveGroup("Quality of Life");
        #endregion
    }

    public class FeatureGroup
    {
        public string Name { get; private set; } = string.Empty;
        internal string DisplayName => _languages.TryGetValue(LocalizationCoreService.CurrentLanguage, out var text) ? text : Name;
        public bool IsNewlyCreated { get; internal set; } = true;
        public bool IsModuleGroup { get; internal set; } = false;
        public bool IsHidden { get; private set; } = false;
        public FeatureGroup ParentGroup { get; internal set; } = null;
        public HashSet<FeatureGroup> SubGroups { get; } = new();
        internal HashSet<Feature> Features { get; } = new();

        private Dictionary<Language, string> _languages = new();

        public FeatureGroup GetOrCreateSubGroup(string name, bool dev = false)
        {
            var subGroup = SubGroups.FirstOrDefault(g => g.Name == name);

            if (subGroup != null)
                IsNewlyCreated = false;

            subGroup ??= new FeatureGroup(name, false, this, dev);

            return subGroup;
        }

        public void SetLanguage(Dictionary<Language, string> languages)
        {
            foreach (var lang in languages)
            {
                _languages[lang.Key] = lang.Value;
            }
        }

        public void SetLanguage(Language language, string text)
        {
            _languages[language] = text;
        }

        internal FeatureGroup(string name, bool moduleGroup = false, FeatureGroup parentGroup = null, bool dev = false)
        {
            Name = name;
            IsHidden = dev;
            if (moduleGroup)
            {
                IsModuleGroup = true;
                FeatureGroups.ModuleGroups.Add(this);
            }
            else if (parentGroup != null)
            {
                ParentGroup = parentGroup;
                ParentGroup.SubGroups.Add(this);
            }
            FeatureGroups.AllGroups.Add(this);
        }

        private FeatureGroup() { }

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator string(FeatureGroup g) => g.Name;
    }
}
