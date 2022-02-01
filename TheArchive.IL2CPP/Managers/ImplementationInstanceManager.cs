using GameData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Managers
{
    /// <summary>
    /// A way to interact with types that have had their name and or namespace changed<br/>
    /// to keep the code compatible across multiple versions (see boosters R5 vs R6 for example)<br/>
    /// or types that reference newer types that don't exist in previous versions (see Localization in R6)
    /// </summary>
    public class ImplementationInstanceManager
    {
        private static Dictionary<Type, object> _implementationInstances = new Dictionary<Type, object>();

        /// <summary>
        /// Looks for a class implementing <typeparamref name="T"/> in the current domain and returns an instance of the first one it finds.<br/>
        /// Basically a factory resolver.<br/><br/>
        /// First time lookup might be slow as it's looping through every loaded type in the current domain.<br/>
        /// Subsequent lookups are returning a cached instance.
        /// </summary>
        /// <typeparam name="T">Implementation to lookup (cached)</typeparam>
        /// <returns>An instance of the first found type that implements <typeparamref name="T"/></returns>
        public static T GetOrFindImplementation<T>()
        {
            if(_implementationInstances.TryGetValue(typeof(T), out var val)) {
                return (T) val;
            }

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if(typeof(T).IsAssignableFrom(type) && typeof(T) != type)
                    {
                        ArchiveLogger.Debug($"Found implementation \"{type.FullName}\" for \"{typeof(T).FullName}\"!");
                        var instance = Activator.CreateInstance(type);
                        _implementationInstances.Add(typeof(T), instance);
                        return (T) instance;
                    }
                }
            }

            throw new ArgumentException($"Could not find implementation for type \"{typeof(T).FullName}\"!");
        }

        /// <summary>
        /// Gets all internaly enabled data blocks of type with name <paramref name="datablockTypeName"/> and converts them to a custom implemention <typeparamref name="T"/> using a factory that implements <see cref="IBaseGameConverter{CT}"/> (where <typeparamref name="CT"/> is <typeparamref name="T"/>).
        /// </summary>
        /// <typeparam name="T">The custom data block equivalent.</typeparam>
        /// <param name="datablockTypeName">The base game data block type name.</param>
        /// <returns>An array of all the enabled data blocks as the custom variant <typeparamref name="T"/>.</returns>
        public static T[] GetAllCustomDataBlocksFor<T>(string datablockTypeName) where T : class, new()
        {
            var getter = GetOrFindImplementation<IBaseGameConverter<T>>();

            var blockType = DataBlockManager.DataBlockTypes.First(t => t.Name == datablockTypeName);

            //GetAllBlocks();
            var allBlocks = typeof(GameDataBlockBase<>).MakeGenericType(new Type[] { blockType }).GetMethod("GetAllBlocks").Invoke(null, new object[0]);

            IEnumerable allBlocksEnumerable = (IEnumerable) allBlocks;

            List<T> allCustomBlocks = new List<T>();
            foreach(var objBlock in allBlocksEnumerable)
            {
                allCustomBlocks.Add(getter.FromBaseGame(objBlock));
            }

            return allCustomBlocks.ToArray();
        }

        /// <summary>
        /// Convert a base game class instance into it's custom equivalent.<br/>
        /// Must have a type implementing <seealso cref="IBaseGameConverter{CT}"/> (where <typeparamref name="CT"/> is <typeparamref name="T"/>) loaded in the current domain to work!
        /// </summary>
        /// <typeparam name="T">Custom type representing the base game one.</typeparam>
        /// <param name="baseGame">The base game instance to copy.</param>
        /// <param name="existingCustom">Optional existing custom instance to set the values on.</param>
        /// <returns>Custom type <typeparamref name="T"/></returns>
        public static T FromBaseGameConverter<T>(object baseGame, T existingCustom = null) where T : class, new()
        {
            return GetOrFindImplementation<IBaseGameConverter<T>>().FromBaseGame(baseGame, existingCustom);
        }

        /// <summary>
        /// Convert a custom class instance into it's base game equivalent.<br/>
        /// Must have a type implementing <seealso cref="IBaseGameConverter{CT}"/> (where <typeparamref name="CT"/> is <typeparamref name="T"/>) loaded in the current domain to work!
        /// </summary>
        /// <typeparam name="T">Custom type representing the base game one.</typeparam>
        /// <param name="custom">The custom instance to copy.</param>
        /// <param name="existingBaseGame">Optional existing base game instance to set the values on.</param>
        /// <returns>The base game equivalent of <typeparamref name="T"/></returns>
        public static object ToBaseGameConverter<T>(T custom, object existingBaseGame = null) where T : class, new()
        {
            return GetOrFindImplementation<IBaseGameConverter<T>>().ToBaseGame(custom, existingBaseGame);
        }

    }
}
