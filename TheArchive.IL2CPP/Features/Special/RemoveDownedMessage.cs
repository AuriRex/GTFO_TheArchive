using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Features.Special
{
    public class RemoveDownedMessage : Feature
    {
        public override string Name => "Remove Downed Message";

        public override string Group => FeatureGroups.Special;

        [ArchivePatch(typeof(PLOC_Downed), nameof(PLOC_Downed.Enter))]
        public class PLOC_Downed_Enter_Patch
        {
            public static bool IsMethodExecuting { get; private set; } = false;
            public static void Prefix() => IsMethodExecuting = true;

            public static void Postfix() => IsMethodExecuting = false;
        }

        [ArchivePatch(typeof(PLOC_Downed), nameof(PLOC_Downed.Exit))]
        public class PLOC_Downed_Exit_Patch
        {
            public static bool IsMethodExecuting { get; private set; } = false;
            public static void Prefix() => IsMethodExecuting = true;

            public static void Postfix() => IsMethodExecuting = false;
        }

        public static bool ShouldSkipSetMessage()
        {
            return PLOC_Downed_Enter_Patch.IsMethodExecuting || PLOC_Downed_Exit_Patch.IsMethodExecuting;
        }

        //GuiManager.InteractionLayer.SetMessage
        [RundownConstraint(Utilities.Utils.RundownFlags.RundownTwo, Utilities.Utils.RundownFlags.Latest)]
        [ArchivePatch(typeof(InteractionGuiLayer), nameof(InteractionGuiLayer.SetMessage), new Type[] { typeof(string), typeof(ePUIMessageStyle), typeof(int) })]
        public class InteractionGuiLayer_SetMessage_Patch_R2Plus
        {
            public static bool Prefix() => !ShouldSkipSetMessage();
        }

        [RundownConstraint(Utilities.Utils.RundownFlags.RundownOne)]
        [ArchivePatch(typeof(InteractionGuiLayer), nameof(InteractionGuiLayer.SetMessage), new Type[] { typeof(string), typeof(ePUIMessageStyle) })]
        public class InteractionGuiLayer_SetMessage_Patch_R1
        {
            public static bool Prefix() => !ShouldSkipSetMessage();
        }

        //GuiManager.InteractionLayer.MessageVisible
        [ArchivePatch(typeof(InteractionGuiLayer), nameof(InteractionGuiLayer.MessageVisible), null, ArchivePatch.PatchMethodType.Setter)]
        public class InteractionGuiLayer_MessageVisible_Patch
        {
            public static bool Prefix()
            {
                return !ShouldSkipSetMessage();
            }
        }
    }
}
