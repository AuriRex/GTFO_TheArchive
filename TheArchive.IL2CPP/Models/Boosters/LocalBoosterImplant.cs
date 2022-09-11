using Newtonsoft.Json;
using System;
using TheArchive.Models.DataBlocks;

namespace TheArchive.Models.Boosters
{
    public class LocalBoosterImplant
    {
        public LocalBoosterImplant()
        {

        }

        // DropServer variant constructor
        public LocalBoosterImplant(uint templateId, uint instanceId, int uses, Effect[] effects, uint[] conditions)
        {
            TemplateId = templateId;
            InstanceId = instanceId;
            Uses = uses;

            Effects = effects ?? new Effect[0];
            Conditions = conditions ?? new uint[0];
        }

        public static void CopyTraitColorsFromBasegame()
        {
            TraitConditionColor = BoosterImplant.TraitConditionColor;
            TraitNegativeColor = BoosterImplant.TraitNegativeColor;
            TraitPositiveColor = BoosterImplant.TraitPositiveColor;
        }

        public static string TraitConditionColor { get; private set; }
        public static string TraitNegativeColor { get; private set; }
        public static string TraitPositiveColor { get; private set; }
        public uint TemplateId { get; set; } // Readonly in basegame
        public A_BoosterImplantCategory Category { get; set; } // Readonly in basegame
        public int Uses { get; set; }
        public Effect[] Effects { get; set; }
        [JsonIgnore]
        [Obsolete("Expect this to be null")]
        public CustomBoosterImplantTemplateDataBlock Template { get; set; }
        public uint InstanceId { get; set; }
        public uint[] Conditions { get; set; }

        public struct Effect
        {
            public uint Id;
            public float Value;
        }

        public enum A_BoosterImplantCategory
        {
            Muted,
            Bold,
            Aggressive
        }
    }
}
