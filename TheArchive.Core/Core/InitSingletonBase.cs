using System;
using TheArchive.Utilities;

namespace TheArchive.Core
{
    public abstract class InitSingletonBase<T> where T : class
    {

        private static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    ArchiveLogger.Warning($"Instance on \"{typeof(T).FullName}\" not set even though it's being accessed.");
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

    }
}
