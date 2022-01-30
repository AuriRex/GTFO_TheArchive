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
    public class ImplementationInstanceManager
    {

        public static Dictionary<Type, object> _implementationInstances = new Dictionary<Type, object>();


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

    }
}
