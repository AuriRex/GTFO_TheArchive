using GameData;
using System;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using TheArchive.Utilities;

namespace TheArchive.Impl.R5.DataBlocks;

[RundownConstraint(Utils.RundownFlags.RundownFive)]
public class CustomBoosterImplantConditionDataBlockConverter : IBaseGameConverter<CustomBoosterImplantConditionDataBlock>
{
    public PropertyInfo Description;
    public PropertyInfo PublicName;
    public PropertyInfo PublicShortName;

    public CustomBoosterImplantConditionDataBlockConverter()
    {
        Description = typeof(BoosterImplantConditionDataBlock).GetProperty(nameof(BoosterImplantConditionDataBlock.Description), Utils.AnyBindingFlagss);
        PublicName = typeof(BoosterImplantConditionDataBlock).GetProperty(nameof(BoosterImplantConditionDataBlock.PublicName), Utils.AnyBindingFlagss);
        PublicShortName = typeof(BoosterImplantConditionDataBlock).GetProperty(nameof(BoosterImplantConditionDataBlock.PublicShortName), Utils.AnyBindingFlagss);
    }

    public CustomBoosterImplantConditionDataBlock FromBaseGame(object baseGame, CustomBoosterImplantConditionDataBlock existingCT = null)
    {
        var baseBlock = (BoosterImplantConditionDataBlock)baseGame;

        var customBlock = existingCT ?? new CustomBoosterImplantConditionDataBlock();

        customBlock = (CustomBoosterImplantConditionDataBlock)ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

        customBlock.Condition = (int)baseBlock.Condition;

        customBlock.Description = Description.GetValue(baseBlock).ToString(); // baseBlock.Description;
        customBlock.PublicName = PublicName.GetValue(baseBlock).ToString(); // baseBlock.PublicName;
        customBlock.PublicShortName = PublicShortName.GetValue(baseBlock).ToString(); // baseBlock.PublicShortName;

        customBlock.IconPath = baseBlock.IconPath;

        return customBlock;
    }

    public Type GetBaseGameType() => typeof(BoosterImplantConditionDataBlock);

    public Type GetCustomType() => typeof(CustomBoosterImplantConditionDataBlock);

    public object ToBaseGame(CustomBoosterImplantConditionDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
}