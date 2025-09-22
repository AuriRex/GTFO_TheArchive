using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.Localization;

namespace TheArchive.Features.Security;

public class BasicPunishmentSettings
{
    [FSDisplayName("Punish Friends")]
    [FSDescription("If (Steam) Friends should be affected as well.")]
    public bool PunishFriends { get; set; } = false;

    [FSDisplayName("Punishment")]
    [FSDescription("What to do with griefers.")]
    public PunishmentMode Punishment { get; set; } = PunishmentMode.Kick;

    [Localized]
    public enum PunishmentMode
    {
        NoneAndLog,
        Kick,
        KickAndBan
    }
}
