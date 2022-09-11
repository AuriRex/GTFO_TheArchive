using DropServer;
using System;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using UnhollowerRuntimeLib;

namespace TheArchive.IL2CPP.R5.Factories
{
    public class CustomBoosterImplantFactory : IBaseGameConverter<LocalBoosterImplant>
    {
        public LocalBoosterImplant FromBaseGame(object baseGame, LocalBoosterImplant existingCBI = null)
        {
            var implant = (BoosterImplantBase) baseGame;

            LocalBoosterImplant.Effect[] effects = new LocalBoosterImplant.Effect[implant.Effects.Length];

            var customImplant = existingCBI ?? new LocalBoosterImplant();

            customImplant.TemplateId = implant.TemplateId;
            customImplant.InstanceId = implant.Id;
            customImplant.Uses = implant.UsesRemaining;
            customImplant.Effects = effects;
            customImplant.Conditions = implant.Conditions;

            return customImplant;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantBase);

        public Type GetCustomType() => typeof(LocalBoosterImplant);

        public object ToBaseGame(LocalBoosterImplant customImplant, object existingBaseGameImplant = null)
        {
            var implant = (BoosterImplantBase) existingBaseGameImplant ?? new BoosterImplantBase(ClassInjector.DerivedConstructorPointer<BoosterImplantBase>());

            implant.Effects = new BoosterImplantEffect[customImplant.Effects.Length];
            for (int i = 0; i < customImplant.Effects.Length; i++)
            {
                var fx = customImplant.Effects[i];
                implant.Effects[i] = new BoosterImplantEffect
                {
                    Id = fx.Id,
                    Param = fx.Value
                };
            }
            implant.TemplateId = customImplant.TemplateId;
            implant.Id = customImplant.InstanceId;
            implant.Conditions = new uint[customImplant.Conditions.Length];
            for (int i = 0; i < customImplant.Conditions.Length; i++)
            {
                implant.Conditions[i] = customImplant.Conditions[i];
            }
            implant.UsesRemaining = customImplant.Uses;

            return implant;
        }
    }
}
