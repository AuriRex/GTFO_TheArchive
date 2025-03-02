using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using TheArchive.Utilities;
#if Unhollower
using UnhollowerBaseLib;
#endif
#if Il2CppInterop
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

namespace TheArchive.Impl.R5.Boosters;

[RundownConstraint(Utils.RundownFlags.RundownFive)]
public class CustomBoosterTransactionConverter : IBaseGameConverter<LocalBoosterTransaction>
{
    public Type BoosterImplantTransaction; //DropServer.BoosterImplantTransaction

    public PropertyInfo MaxBackendTemplateId; // uint
    public PropertyInfo AcknowledgeIds; // Il2CppStructArray<uint>
    public PropertyInfo TouchIds; // Il2CppStructArray<uint>
    public PropertyInfo DropIds; // Il2CppStructArray<uint>
    public PropertyInfo AcknowledgeMissed; //  BoosterImplantTransaction.Missed

    private MethodInfo Il2CppStructArray_uint_OP;

    public CustomBoosterTransactionConverter()
    {
        BoosterImplantTransaction = ImplementationManager.FindTypeInCurrentAppDomain("DropServer.BoosterImplantTransaction", exactMatch: true);

        MaxBackendTemplateId = BoosterImplantTransaction.GetProperty(nameof(MaxBackendTemplateId), Utils.AnyBindingFlagss);
        AcknowledgeIds = BoosterImplantTransaction.GetProperty(nameof(AcknowledgeIds), Utils.AnyBindingFlagss);
        TouchIds = BoosterImplantTransaction.GetProperty(nameof(TouchIds), Utils.AnyBindingFlagss);
        DropIds = BoosterImplantTransaction.GetProperty(nameof(DropIds), Utils.AnyBindingFlagss);
        AcknowledgeMissed = BoosterImplantTransaction.GetProperty(nameof(AcknowledgeMissed), Utils.AnyBindingFlagss);

        Il2CppStructArray_uint_OP = typeof(Il2CppStructArray<>).MakeGenericType(typeof(uint)).GetMethod("op_Implicit", Utils.AnyBindingFlagss);
    }

    public LocalBoosterTransaction FromBaseGame(object baseGame, LocalBoosterTransaction existingCBT = null)
    {
        var boosterTrans = baseGame;

        var customTrans = existingCBT ?? new LocalBoosterTransaction();

        customTrans.MaxBackendTemplateId = (uint) MaxBackendTemplateId.GetValue(boosterTrans);

        customTrans.AcknowledgeIds = ((IEnumerable<uint>)AcknowledgeIds.GetValue(boosterTrans))?.ToArray() ?? Array.Empty<uint>();
        customTrans.DropIds = ((IEnumerable<uint>)DropIds.GetValue(boosterTrans))?.ToArray() ?? Array.Empty<uint>();
        customTrans.TouchIds = ((IEnumerable<uint>)TouchIds.GetValue(boosterTrans))?.ToArray() ?? Array.Empty<uint>();

        customTrans.AcknowledgeMissed = LocalBoosterTransaction.CustomMissed.FromBaseGame(AcknowledgeMissed.GetValue(boosterTrans));

        return customTrans;
    }

    public Type GetBaseGameType() => BoosterImplantTransaction;

    public Type GetCustomType() => typeof(LocalBoosterTransaction);

    public object ToBaseGame(LocalBoosterTransaction customTrans, object existingBaseGame = null)
    {
        var boosterTrans = existingBaseGame ?? Activator.CreateInstance(BoosterImplantTransaction);

        MaxBackendTemplateId.SetValue(boosterTrans, customTrans.MaxBackendTemplateId);

        AcknowledgeIds.SetValue(boosterTrans, Il2CppStructArray_uint_OP.Invoke(null, new object[] { customTrans.AcknowledgeIds }));
        DropIds.SetValue(boosterTrans, Il2CppStructArray_uint_OP.Invoke(null, new object[] { customTrans.DropIds }));
        TouchIds.SetValue(boosterTrans, Il2CppStructArray_uint_OP.Invoke(null, new object[] { customTrans.TouchIds }));

        AcknowledgeMissed.SetValue(boosterTrans, customTrans.AcknowledgeMissed.ToBaseGame());

        return boosterTrans;
    }
}