using GameData;
using Newtonsoft.Json;
using System;
using TheArchive.Models.DataBlocks;

namespace TheArchive.Models.Boosters
{
    public class CustomBoosterImplant
    {
        public CustomBoosterImplant()
        {

        }

        // DropServer variant constructor
        public CustomBoosterImplant(uint templateId, uint instanceId, int uses, Effect[] effects, uint[] conditions)
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
        public BoosterImplantCategory Category { get; set; } // Readonly in basegame
        public int Uses { get; set; }
        public Effect[] Effects { get; set; }
        [JsonIgnore]
        [Obsolete("Expect this to be null for everything that has not been created with an BoosterImplant object as prefab")]
        public CustomBoosterImplantTemplateDataBlock Template { get; set; }
        public uint InstanceId { get; set; }
        public uint[] Conditions { get; set; }

        public struct Effect
        {
            public uint Id;
            public float Value;
        }
    }
}
