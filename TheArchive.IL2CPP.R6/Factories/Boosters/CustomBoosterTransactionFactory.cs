using DropServer.BoosterImplants;
using System;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using static TheArchive.Loader.LoaderWrapper;

namespace TheArchive.IL2CPP.R6.Factories
{
    public class CustomBoosterTransactionFactory : IBaseGameConverter<LocalBoosterTransaction>
    {
        public LocalBoosterTransaction FromBaseGame(object baseGame, LocalBoosterTransaction existingCBT = null)
        {
            var boosterTrans = (BoosterImplantTransaction) baseGame;

            var customTrans = existingCBT ?? new LocalBoosterTransaction();

            customTrans.AcknowledgeIds = boosterTrans.AcknowledgeIds;
            customTrans.DropIds = boosterTrans.DropIds;
            customTrans.MaxBackendTemplateId = boosterTrans.MaxBackendTemplateId;
            customTrans.TouchIds = boosterTrans.TouchIds;

            customTrans.AcknowledgeMissed = LocalBoosterTransaction.CustomMissed.FromBaseGame(boosterTrans.AcknowledgeMissed);

            return customTrans;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantTransaction);

        public Type GetCustomType() => typeof(LocalBoosterTransaction);

        public object ToBaseGame(LocalBoosterTransaction customTrans, object existingBaseGame = null)
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
