using BoosterImplants;
using DropServer;
using GameData;
using Newtonsoft.Json;
using System;
using UnhollowerRuntimeLib;

namespace TheArchive.Models.Boosters
{
    public class CustomBoosterImplant
    {
        internal CustomBoosterImplant()
        {

        }

        // DropServer variant constructor
        internal CustomBoosterImplant(uint templateId, uint instanceId, int uses, Effect[] effects, uint[] conditions)
        {
            TemplateId = templateId;
            InstanceId = instanceId;
            Uses = uses;

            Effects = effects ?? new Effect[0];
            Conditions = conditions ?? new uint[0];
        }

#warning TODO
        /*public CustomBoosterImplant(BoosterImplantBase item)
        {
            SetFromBaseGameImplantBase(item);
        }

        public CustomBoosterImplant(pBoosterImplantData implantData)
        {
            SetFromBaseGamePacket(implantData);
        }

        public CustomBoosterImplant(BoosterImplant implant)
        {
            SetFromBaseGameImplant(implant);
        }

        public void SetFromBaseGameImplant(BoosterImplant implant)
        {
            if (implant == null)
            {
                throw new ArgumentException(nameof(implant));
            }

            TemplateId = implant.TemplateId;
            Category = implant.Category;
            Uses = implant.Uses;
            Effects = new Effect[implant.Effects.Count];
            for (int i = 0; i < implant.Effects.Count; i++)
            {
                var effect = implant.Effects[i];
                Effects[i] = new Effect
                {
                    Id = effect.Id,
                    Value = effect.Value
                };
            }
#pragma warning disable CS0618 // Type or member is obsolete
            Template = implant.Template;
#pragma warning restore CS0618 // Type or member is obsolete
            InstanceId = implant.InstanceId;
            Conditions = new uint[implant.Conditions.Count];
            for (int i = 0; i < implant.Conditions.Count; i++)
            {
                Conditions[i] = implant.Conditions[i];
            }
        }

        public void SetFromBaseGameImplantBase(BoosterImplantBase item)
        {
            if (item == null)
            {
                throw new ArgumentException(nameof(item));
            }

            Effects = new Effect[item.Effects.Count];
            for (int i = 0; i < item.Effects.Count; i++)
            {
                var fx = item.Effects[i];
                Effects[i] = new Effect
                {
                    Id = fx.Id,
                    Value = fx.Param
                };
            }
            TemplateId = item.TemplateId;
            InstanceId = item.Id;
            Conditions = new uint[item.Conditions.Count];
            for (int i = 0; i < item.Conditions.Count; i++)
            {
                Conditions[i] = item.Conditions[i];
            }
            Uses = item.UsesRemaining;
        }

        public void SetFromBaseGamePacket(pBoosterImplantData implantData)
        {
            if(implantData == null)
            {
                throw new ArgumentException(nameof(implantData));
            }

            Effects = new Effect[implantData.BoosterEffectCount];
            for (int i = 0; i < implantData.BoosterEffectCount; i++)
            {
                var data = implantData.BoosterEffectDatas[i];
                Effects[i] = new Effect
                {
                    Id = data.BoosterEffectID,
                    Value = data.EffectValue
                };
            }
            TemplateId = implantData.BoosterImplantID;
            Conditions = new uint[implantData.ConditionCount];
            for (int i = 0; i < implantData.ConditionCount; i++)
            {
                Conditions[i] = implantData.Conditions[i];
            }
            Uses = implantData.UseCount;
        }

        public void SetToDropServerBaseGame(ref DropServer.BoosterImplantBase implant)
        {
            implant.Effects = new BoosterImplantEffect[Effects.Length];
            for (int i = 0; i < Effects.Length; i++)
            {
                var fx = Effects[i];
                implant.Effects[i] = new BoosterImplantEffect
                {
                    Id = fx.Id,
                    Param = fx.Value
                };
            }
            implant.TemplateId = TemplateId;
            implant.Id = InstanceId;
            implant.Conditions = new uint[Conditions.Length];
            for (int i = 0; i < Conditions.Length; i++)
            {
                implant.Conditions[i] = Conditions[i];
            }
            implant.UsesRemaining = Uses;
        }

        protected DropServer.BoosterImplantInventoryItem ToBaseGameInventoryItem()
        {
            var biii = (DropServer.BoosterImplantBase) new DropServer.BoosterImplantInventoryItem(ClassInjector.DerivedConstructorPointer<DropServer.BoosterImplantInventoryItem>());

            SetToDropServerBaseGame(ref biii);

            return (DropServer.BoosterImplantInventoryItem) biii;
        }*/

        public static void CopyTraitColorsFromBasegame()
        {
            TraitConditionColor = BoosterImplant.TraitConditionColor;
            TraitNegativeColor = BoosterImplant.TraitNegativeColor;
            TraitPositiveColor = BoosterImplant.TraitPositiveColor;
        }

        public static string TraitConditionColor { get; set; }
        public static string TraitNegativeColor { get; set; }
        public static string TraitPositiveColor { get; set; }
        public uint TemplateId { get; set; } // Readonly in basegame
        public BoosterImplantCategory Category { get; set; } // Readonly in basegame
        public int Uses { get; set; }
        public Effect[] Effects { get; set; }
        [JsonIgnore]
        [Obsolete("Expect this to be null for everything that has not been created with an BoosterImplant object as prefab")]
        public BoosterImplantTemplateDataBlock Template { get; set; }
        public uint InstanceId { get; set; }
        public uint[] Conditions { get; set; }

        public struct Effect
        {
            public uint Id;
            public float Value;
        }
    }
}
