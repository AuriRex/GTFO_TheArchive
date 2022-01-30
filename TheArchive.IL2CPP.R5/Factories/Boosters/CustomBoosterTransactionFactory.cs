using DropServer;
using System;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using UnhollowerRuntimeLib;

namespace TheArchive.IL2CPP.R5.Factories
{
    public class CustomBoosterTransactionFactory : IBaseGameConverter<CustomBoosterTransaction>
    {
        public CustomBoosterTransaction FromBaseGame(object baseGame, CustomBoosterTransaction existingCBT = null)
        {
            var boosterTrans = (BoosterImplantTransaction) baseGame;

            var customTrans = existingCBT ?? new CustomBoosterTransaction();

            customTrans.AcknowledgeIds = boosterTrans.AcknowledgeIds;
            customTrans.DropIds = boosterTrans.DropIds;
            customTrans.MaxBackendTemplateId = boosterTrans.MaxBackendTemplateId;
            customTrans.TouchIds = boosterTrans.TouchIds;

            customTrans.AcknowledgeMissed = CustomBoosterTransaction.CustomMissed.FromBaseGame(boosterTrans.AcknowledgeMissed);

            return customTrans;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantTransaction);

        public Type GetCustomType() => typeof(CustomBoosterTransaction);

        public object ToBaseGame(CustomBoosterTransaction customTrans, object existingBaseGame = null)
        {
            var boosterTrans = (BoosterImplantTransaction) existingBaseGame ?? new BoosterImplantTransaction(ClassInjector.DerivedConstructorPointer<BoosterImplantTransaction>());

            boosterTrans.AcknowledgeIds = customTrans.AcknowledgeIds;
            boosterTrans.DropIds = customTrans.DropIds;
            boosterTrans.MaxBackendTemplateId = customTrans.MaxBackendTemplateId;
            boosterTrans.TouchIds = customTrans.TouchIds;

            boosterTrans.AcknowledgeMissed = (BoosterImplantTransaction.Missed) customTrans.AcknowledgeMissed.ToBaseGame();

            return boosterTrans;
        }
    }
}
