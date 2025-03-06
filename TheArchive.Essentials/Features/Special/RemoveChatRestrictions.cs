using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;
#if Unhollower
using UnhollowerBaseLib;
#endif
#if Il2CppInterop
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

namespace TheArchive.Features.Special;

[RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
public class RemoveChatRestrictions : Feature
{
    public override string Name => "Remove Chat Restrictions";

    public override FeatureGroup Group => FeatureGroups.Special;

    public override string Description => "Allows the usage of '>' and '<' characters in chat.\n\n(Also enables TextMeshPro RichText tags to be used in chat, don't do stupid things!)";

    public new static IArchiveLogger FeatureLogger { get; set; }
    
#if IL2CPP
    private static PropertyAccessor<PlayerChatManager, Il2CppStructArray<int>> A_PlayerChatManager_m_forbiddenChars;
#else
        private static FieldAccessor<PlayerChatManager, int[]> A_PlayerChatManager_m_forbiddenChars;
#endif

    private readonly int[] _forbiddenChars = new int[]
    {
        60,
        61,
        62
    };

    public override void OnEnable()
    {
        SetValues(PlayerChatManager.Current);
    }

    public override void OnDisable()
    {
        SetValues(PlayerChatManager.Current, _forbiddenChars);
    }

    public override void Init()
    {
        A_PlayerChatManager_m_forbiddenChars =
#if IL2CPP
            PropertyAccessor<PlayerChatManager, Il2CppStructArray<int>>
#else
                FieldAccessor<PlayerChatManager, int[]>
#endif
                .GetAccessor("m_forbiddenChars");
    }

    public static void SetValues(PlayerChatManager instance, int[] values = null)
    {
        if (instance == null) return;

        A_PlayerChatManager_m_forbiddenChars.Set(instance, values ?? Array.Empty<int>());
    }

    // Remove the character restriction in chat, this also results in the user being able to use TMP tags but whatever
    [ArchivePatch(typeof(PlayerChatManager), nameof(PlayerChatManager.Setup))]
    internal static class PlayerChatManager_SetupPatch
    {
        public static void Postfix(ref PlayerChatManager __instance)
        {
            try
            {
                SetValues(__instance);
            }
            catch (Exception ex)
            {
                FeatureLogger.Exception(ex);
            }
        }
    }
}