using System.Collections.Generic;
using TheArchive.Core.Managers;
using UnityEngine;
using static TheArchive.Models.Boosters.LocalBoosterImplant;

namespace TheArchive.Models.DataBlocks
{
    public class CustomBoosterImplantTemplateDataBlock : CustomGameDataBlockBase
    {

        public bool Deprecated { get; set; }
        public string PublicName { get; set; }
        public string Description { get; set; }
        public Vector2 DurationRange { get; set; }
        public float DropWeight { get; set; }
        public List<uint> Conditions { get; set; }
        public List<uint> RandomConditions { get; set; }
        public List<A_BoosterImplantEffectInstance> Effects { get; set; }
        public List<List<A_BoosterImplantEffectInstance>> RandomEffects { get; set; }
        // enum (BoosterEffectCategory)
        public int MainEffectType { get; set; }
        public A_BoosterImplantCategory ImplantCategory { get; set; }

        public object ToBaseGame() => ToBaseGame(this);

        public static object ToBaseGame(CustomBoosterImplantTemplateDataBlock custom)
        {
            return ImplementationManager.ToBaseGameConverter(custom);
        }

        public static CustomBoosterImplantTemplateDataBlock FromBaseGame(object baseGame)
        {
            return ImplementationManager.FromBaseGameConverter<CustomBoosterImplantTemplateDataBlock>(baseGame);
        }

        public class A_BoosterImplantEffectInstance
        {
            public uint BoosterImplantEffect { get; set; }
            public float MinValue { get; set; }
            public float MaxValue { get; set; }
        }
    }
}
