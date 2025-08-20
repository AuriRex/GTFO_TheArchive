using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Core;

/// <summary>
/// A singleton thingie.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class InitSingletonBase<T> where T : class
{
    /// <summary>
    /// True if the <see cref="IInitializable.Init"/> method has been executed successfully
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    public static bool HasBeenInitialized { get; private set; } = false;

    private static T _instance;
    /// <summary>
    /// The Singleton instance.
    /// </summary>
    /// <remarks>
    /// Gets set automatically.
    /// </remarks>
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