using SNetwork;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Models;
using TheArchive.Utilities;

namespace TheArchive.Features.Accessibility
{
    public class Nickname : Feature
    {
        public override string Name => "Nickname";

        public override string Group => FeatureGroups.Accessibility;

        public class NicknameSettings
        {
            public NicknameMode Mode { get; set; } = NicknameMode.Normal;
            [FSMaxLength(25), FSDisplayName("Nick (25) no Color")]
            public string Nick25 { get; set; }
            public SColor Color { get; set; } = new SColor(0f, 1f, 0.75f);
            [FSMaxLength(13), FSDisplayName("Nick (13) ++ Color")]
            public string Nick13 { get; set; }
            [FSMaxLength(5), FSDisplayName("Nick (5) + Color")]
            public string Nick5 { get; set; }
        }

        [FeatureConfig]
        public static NicknameSettings Settings { get; set; }

        public override void OnEnable()
        {
            SetNickname();
        }

#warning TODO: OnGameState -> apply nickname

        public override void OnDisable()
        {
            SNet.LocalPlayer.NickName = string.Empty;
        }

        public static void SetNickname()
        {
            if(SNet.LocalPlayer == null)
            {
                return;
            }

            switch(Settings.Mode)
            {
                default:
                case NicknameMode.Normal:
                    if(!string.IsNullOrWhiteSpace(Settings.Nick25))
                    {
                        SNet.LocalPlayer.NickName = Settings.Nick25;
                        return;
                    }
                    
                    break;
                case NicknameMode.ColorWithOverflow:
                    if(!string.IsNullOrWhiteSpace(Settings.Nick13))
                    {
                        SNet.LocalPlayer.NickName = $"<color={Settings.Color.ToShortHexString()}>{Settings.Nick13}";
                        return;
                    }
                    break;
                case NicknameMode.Color:
                    if (!string.IsNullOrWhiteSpace(Settings.Nick5))
                    {
                        SNet.LocalPlayer.NickName = $"<color={Settings.Color.ToShortHexString()}>{Settings.Nick5}</color>";
                        return;
                    }
                    break;
            }

            SNet.LocalPlayer.NickName = string.Empty;
        }

        public enum NicknameMode
        {
            Normal,
            ColorWithOverflow,
            Color
        }
    }
}
