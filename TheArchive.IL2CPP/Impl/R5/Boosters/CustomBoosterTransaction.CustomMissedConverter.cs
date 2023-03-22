using System;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Models.Boosters.LocalBoosterTransaction;

namespace TheArchive.Impl.R5.Boosters
{
    [RundownConstraint(Utils.RundownFlags.RundownFive)]
    public class CustomMissedConverter : IBaseGameConverter<CustomMissed>
    {
        private Type BoosterImplantTransaction;
        private Type BoosterImplantTransaction_Missed;

        private PropertyInfo Basic; // int
        private PropertyInfo Advanced; // int
        private PropertyInfo Specialized; // int

        public CustomMissedConverter()
        {
            BoosterImplantTransaction = ImplementationManager.FindTypeInCurrentAppDomain("DropServer.BoosterImplantTransaction", exactMatch: true);

            //DropServer.BoosterImplantTransaction.Missed
            BoosterImplantTransaction_Missed = BoosterImplantTransaction.GetNestedType("Missed", Utils.AnyBindingFlagss);

            Basic = BoosterImplantTransaction_Missed.GetProperty(nameof(Basic), Utils.AnyBindingFlagss);
            Advanced = BoosterImplantTransaction_Missed.GetProperty(nameof(Advanced), Utils.AnyBindingFlagss);
            Specialized = BoosterImplantTransaction_Missed.GetProperty(nameof(Specialized), Utils.AnyBindingFlagss);
        }

        public CustomMissed FromBaseGame(object baseGame, CustomMissed existingCM = null)
        {
            var baseGameMissed = baseGame;

            var customMissed = existingCM ?? new CustomMissed();

            customMissed.Basic = (int) Basic.GetValue(baseGameMissed);
            customMissed.Advanced = (int) Advanced.GetValue(baseGameMissed);
            customMissed.Specialized = (int) Specialized.GetValue(baseGameMissed);

            return customMissed;
        }

        public Type GetBaseGameType() => BoosterImplantTransaction_Missed;

        public Type GetCustomType() => typeof(CustomMissed);

        public object ToBaseGame(CustomMissed customMissed, object existingBaseGame = null)
        {
            var baseGameMissed = existingBaseGame ?? Activator.CreateInstance(BoosterImplantTransaction_Missed);

            Basic.SetValue(baseGameMissed, customMissed.Basic);
            Advanced.SetValue(baseGameMissed, customMissed.Advanced);
            Specialized.SetValue(baseGameMissed, customMissed.Specialized);

            return baseGameMissed;
        }
    }
}
