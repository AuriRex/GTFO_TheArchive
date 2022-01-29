using System;

namespace TheArchive.Interfaces
{
    public interface IBaseGameConverter<CT>
    {
        public CT FromBaseGame(object baseGame);

        public object ToBaseGame(CT customType, object existingBaseGame = null);

        public Type GetBaseGameType();

        public Type GetCustomType();
    }
}
