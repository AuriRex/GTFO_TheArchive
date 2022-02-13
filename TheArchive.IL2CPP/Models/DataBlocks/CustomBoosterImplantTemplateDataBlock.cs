using GameData;
using System.Collections.Generic;
using TheArchive.Core.Managers;
using UnityEngine;

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
        public List<BoosterImplantEffectInstance> Effects { get; set; }
        public List<List<BoosterImplantEffectInstance>> RandomEffects { get; set; }
        public BoosterEffectCategory MainEffectType { get; set; }
        public BoosterImplantCategory ImplantCategory { get; set; }

        public object ToBaseGame() => ToBaseGame(this);

        public static object ToBaseGame(CustomBoosterImplantTemplateDataBlock custom)
        {
            return ImplementationManager.ToBaseGameConverter(custom);
        }

        public static CustomBoosterImplantTemplateDataBlock FromBaseGame(object baseGame)
        {
            return ImplementationManager.FromBaseGameConverter<CustomBoosterImplantTemplateDataBlock>(baseGame);
        }

    }
}
