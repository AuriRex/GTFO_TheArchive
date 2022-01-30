using GameData;
using System;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;

namespace TheArchive.IL2CPP.R5.Factories.DataBlocks
{
    public class CustomGameDataBlockBaseFactory : IBaseGameConverter<CustomGameDataBlockBase>
    {
        public CustomGameDataBlockBase FromBaseGame(object baseGame, CustomGameDataBlockBase existingCT = null)
        {
            if (existingCT == null) throw new ArgumentException($"{nameof(CustomGameDataBlockBase)} can't be used on it's own and must be provided with an existing instance!");

            existingCT.InternalEnabled = (bool) baseGame.GetType().GetProperty("internalEnabled").GetValue(baseGame);
            existingCT.Name = baseGame.GetType().GetProperty("name").GetValue(baseGame).ToString();
            existingCT.PersistentID = (uint) baseGame.GetType().GetProperty("persistentID").GetValue(baseGame);

            return existingCT;
        }
        public Type GetBaseGameType() => typeof(GameDataBlockBase<>);
        public Type GetCustomType() => typeof(CustomGameDataBlockBase);
        public object ToBaseGame(CustomGameDataBlockBase customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
