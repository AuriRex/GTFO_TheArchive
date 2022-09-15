using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Core
{
    public abstract class InitSingletonBase<T> where T : class
    {
        /// <summary>
        /// True if the <see cref="IInitializable.Init"/> method has been executed successfully
        /// </summary>
        public static bool HasBeenInitialized { get; private set; } = false;

        private static T _instance = null;
        /// <summary>
        /// The Singleton instance <see cref="T"/>
        /// <br/>
        /// Gets set automatically
        /// </summary>
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
