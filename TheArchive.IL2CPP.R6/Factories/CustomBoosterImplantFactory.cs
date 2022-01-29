using DropServer.BoosterImplants;
using System;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using UnhollowerRuntimeLib;

namespace TheArchive.IL2CPP.R6.Factories
{
    public class CustomBoosterImplantFactory : IBaseGameConverter<CustomBoosterImplant>
    {
        public CustomBoosterImplant FromBaseGame(object baseGame)
        {
            var implant = (BoosterImplantBase) baseGame;

            CustomBoosterImplant.Effect[] effects = new CustomBoosterImplant.Effect[implant.Effects.Length];

            var customImplant = new CustomBoosterImplant(
                implant.TemplateId,
                implant.Id,
                implant.UsesRemaining,
                effects,
                implant.Conditions
                );

            return customImplant;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantBase);

        public Type GetCustomType() => typeof(CustomBoosterImplant);

        public object ToBaseGame(CustomBoosterImplant customImplant, object existingBaseGameImplant = null)
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
