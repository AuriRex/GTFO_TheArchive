using System;

namespace TheArchive.Interfaces
{
    public interface IBaseGameConverter<CT> where CT : class, new()
    {
        public CT FromBaseGame(object baseGame, CT existingCT = null);

        public object ToBaseGame(CT customType, object existingBaseGame = null);

        public Type GetBaseGameType();

        public Type GetCustomType();
    }
}
