using System;
using System.Collections;
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
public class CustomBoosterImplantConverter : IBaseGameConverter<LocalBoosterImplant>
{
    public Type BoosterImplantBase;

    public PropertyInfo TemplateId; // uint
    public PropertyInfo Id; // uint
    public PropertyInfo UsesRemaining; // int
    public PropertyInfo Effects; // IEnumerable
    public PropertyInfo Conditions; // Il2CppStructArray<uint>


    public Type BoosterImplantEffect;

    public FieldInfo Param; // float
    public FieldInfo FX_Id; // uint

    public MethodInfo Il2CppStructArray_BoosterImplantEffect_OP;
    public MethodInfo Il2CppStructArray_uint_OP;

    public CustomBoosterImplantConverter()
    {
        BoosterImplantBase = ImplementationManager.FindTypeInCurrentAppDomain("DropServer.BoosterImplantBase", exactMatch: true);

        TemplateId = BoosterImplantBase.GetProperty(nameof(TemplateId), Utils.AnyBindingFlagss);
        Id = BoosterImplantBase.GetProperty(nameof(Id), Utils.AnyBindingFlagss);
        UsesRemaining = BoosterImplantBase.GetProperty(nameof(UsesRemaining), Utils.AnyBindingFlagss);
        Effects = BoosterImplantBase.GetProperty(nameof(Effects), Utils.AnyBindingFlagss);
        Conditions = BoosterImplantBase.GetProperty(nameof(Conditions), Utils.AnyBindingFlagss);


        BoosterImplantEffect = ImplementationManager.FindTypeInCurrentAppDomain("DropServer.BoosterImplantEffect", exactMatch: true);

        Param = BoosterImplantEffect.GetField(nameof(Param), Utils.AnyBindingFlagss);
        FX_Id = BoosterImplantEffect.GetField("Id", Utils.AnyBindingFlagss);


        Il2CppStructArray_BoosterImplantEffect_OP = typeof(Il2CppStructArray<>).MakeGenericType(BoosterImplantEffect).GetMethod("op_Implicit", Utils.AnyBindingFlagss);
        Il2CppStructArray_uint_OP = typeof(Il2CppStructArray<>).MakeGenericType(typeof(uint)).GetMethod("op_Implicit", Utils.AnyBindingFlagss);
    }

    public LocalBoosterImplant FromBaseGame(object baseGame, LocalBoosterImplant existingCBI = null)
    {
        var implant = baseGame;

        var customImplant = existingCBI ?? new LocalBoosterImplant();

        var effectList = new List<LocalBoosterImplant.Effect>();
        IEnumerable effectEnumerable = (IEnumerable)Effects.GetValue(implant, null);

        foreach (object effect in effectEnumerable)
        {
            effectList.Add(
                new LocalBoosterImplant.Effect
                {
                    Value = (float) Param.GetValue(effect),
                    Id = (uint) FX_Id.GetValue(effect),
                });
        }

        customImplant.TemplateId = (uint) TemplateId.GetValue(implant);
        customImplant.InstanceId = (uint) Id.GetValue(implant);
        customImplant.Uses = (int) UsesRemaining.GetValue(implant);
        customImplant.Effects = effectList.ToArray();
        customImplant.Conditions = ((IEnumerable<uint>)Conditions.GetValue(implant)).ToArray();

        return customImplant;
    }

    public Type GetBaseGameType() => BoosterImplantBase;

    public Type GetCustomType() => typeof(LocalBoosterImplant);

    public object ToBaseGame(LocalBoosterImplant customImplant, object existingBaseGameImplant = null)
    {
        var implant = existingBaseGameImplant ?? Activator.CreateInstance(BoosterImplantBase);

        // effects = new BoosterImplantEffect[customImplant.Effects.Length]
        Array effects = (Array) Activator.CreateInstance(BoosterImplantEffect.MakeArrayType(), new object[] { customImplant.Effects.Length });

        for (int i = 0; i < customImplant.Effects.Length; i++)
        {
            var fx = customImplant.Effects[i];

            var newVal = Activator.CreateInstance(BoosterImplantEffect);

            FX_Id.SetValue(newVal, fx.Id);

            Param.SetValue(newVal, fx.Value);

            effects.SetValue(newVal, i);
        }

        // call the implicit operator that converts an array to an Il2CppStructArray
        // implant.Effects = (Il2CppStructArray<BoosterImplantEffect>) effects
        Effects.SetValue(implant, Il2CppStructArray_BoosterImplantEffect_OP.Invoke(null, new object[] { effects }));

        TemplateId.SetValue(implant, customImplant.TemplateId);
        Id.SetValue(implant, customImplant.InstanceId);


        // call the implicit operator that converts an array to an Il2CppStructArray
        Conditions.SetValue(implant, Il2CppStructArray_uint_OP.Invoke(null, new object[] { customImplant.Conditions }));

        UsesRemaining.SetValue(implant, customImplant.Uses);

        return implant;
    }
}