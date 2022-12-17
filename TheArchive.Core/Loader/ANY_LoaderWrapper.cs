using System;

namespace TheArchive.Loader
{
    public static partial class LoaderWrapper
    {
        public static bool IsIL2CPPType(Type type)
        {
            if (!IsGameIL2CPP())
                return false;

            return ArchiveMod.IL2CPP_BaseType.IsAssignableFrom(type);
        }
    }
}
