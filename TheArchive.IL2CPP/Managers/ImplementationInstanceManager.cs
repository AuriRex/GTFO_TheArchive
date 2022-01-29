using System;
using System.Collections.Generic;
using System.Reflection;
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

            throw new ArgumentException($"Could not find implementation for type \"{nameof(T)}\"!");
        }

    }
}
