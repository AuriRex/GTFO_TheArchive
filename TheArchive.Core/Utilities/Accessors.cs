using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TheArchive.Utilities
{
    /// <summary>
    /// AccessorBase, contains a static Dictionary containing all cached accessors.
    /// </summary>
    public abstract class AccessorBase
    {
        public static object[] NoParams { get; private set; } = new object[0];
        public static BindingFlags AnyBindingFlags => Core.ArchivePatcher.AnyBindingFlags;

        protected static readonly Dictionary<string, AccessorBase> Accessors = new Dictionary<string, AccessorBase>();

        /// <summary>
        /// Identifies the reflected member.<br/>
        /// </summary>
        public string Identifier { get; private set; } = null;

        public abstract bool HasMemberBeenFound { get; }

        protected AccessorBase(string identifier)
        {
            Identifier = identifier;
        }
    }

    /// <summary>
    /// Globally cached reflection wrapper for fields.
    /// </summary>
    /// <typeparam name="T">The Type that the field belongs to</typeparam>
    /// <typeparam name="FT">The Type of the field itself</typeparam>
    public class FieldAccessor<T, FT> : AccessorBase
    {
        /// <summary>
        /// Gets a <see cref="FieldAccessor{T, FT}"/> from the global cache or creates a new instance if there is none and adds it to the cache.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns><see cref="FieldAccessor{T, FT}"/></returns>
        public static FieldAccessor<T, FT> GetAccessor(string fieldName)
        {
            var identifier = $"Field_{typeof(T).FullName}_{fieldName}";

            if (Accessors.TryGetValue(identifier, out var val))
                return (FieldAccessor<T, FT>) val;

            val = new FieldAccessor<T, FT>(identifier, fieldName);

            Accessors.Add(identifier, val);

            return (FieldAccessor<T, FT>) val;
        }

        private readonly FieldInfo _field;

        public override bool HasMemberBeenFound => _field != null;

        private FieldAccessor(string identifier, string fieldName) : base(identifier)
        {
            _field = typeof(T).GetField(fieldName, AnyBindingFlags);
        }

        /// <summary>
        /// Get the value of the reflected field from an <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An object instance to get the value from</param>
        /// <returns></returns>
        public FT Get(T instance)
        {
            try
            {
                return (FT)_field.GetValue(instance);
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Exception while getting {nameof(FieldAccessor<T, FT>)} field \"{Identifier}\"!");
                ArchiveLogger.Exception(ex);
            }
            return default;
        }

        /// <summary>
        /// Set the <paramref name="value"/> of the reflected field on an <paramref name="instance"/>. 
        /// </summary>
        /// <param name="instance">An object instance to set the value of</param>
        /// <param name="value">The new value</param>
        public void Set(T instance, FT value)
        {
            try
            {
                _field.SetValue(instance, value);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Exception while setting {nameof(FieldAccessor<T, FT>)} field \"{Identifier}\"!");
                ArchiveLogger.Exception(ex);
            }
        }
    }

    /// <summary>
    /// Globally cached reflection wrapper for methods.
    /// </summary>
    /// <typeparam name="T">The Type that the method belongs to</typeparam>
    /// <typeparam name="RT">The returned Type (use object for void)</typeparam>
    public class MethodAccessor<T, RT> : AccessorBase
    {
        /// <summary>
        /// Gets a <see cref="MethodAccessor{T, RT}"/> from the global cache or creates a new instance if there is none and adds it to the cache.
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <param name="parameterTypes">Parameter Types of the method (leave null if there are none)</param>
        /// <returns><see cref="MethodAccessor{T, RT}"/></returns>
        public static MethodAccessor<T, RT> GetAccessor(string methodName, Type[] parameterTypes = null)
        {
            var identifier = $"Method_{typeof(T).FullName}_{typeof(RT)}_{methodName}";
            if(parameterTypes != null)
            {
                identifier += $"_{string.Join("_", parameterTypes.Select(pt => pt.Name))}";
            }

            if (Accessors.TryGetValue(identifier, out var val))
                return (MethodAccessor<T, RT>) val;

            val = new MethodAccessor<T, RT>(identifier, methodName, parameterTypes);

            Accessors.Add(identifier, val);

            return (MethodAccessor<T, RT>) val;
        }

        private readonly MethodInfo _method;
        public bool IsMethodStatic => _method.IsStatic;
        public override bool HasMemberBeenFound => _method != null;

        private MethodAccessor(string identifier, string methodName, Type[] parameterTypes) : base(identifier)
        {
            try
            {
                if (parameterTypes == null)
                {
                    _method = typeof(T).GetMethod(methodName, AnyBindingFlags);
                }
                else
                {
                    _method = typeof(T).GetMethod(methodName, AnyBindingFlags, null, parameterTypes, null);
                }
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Method \"{methodName}\" in Type {typeof(T).FullName} could not be resolved on {ex.Source}!");
                ArchiveLogger.Exception(ex);
            }
        }

        /// <summary>
        /// Invoke the reflected method on an <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An object instance to invoke the method on (leave null if method is static)</param>
        /// <param name="parameters">(Optional) method parameters</param>
        /// <returns></returns>
        public RT Invoke(T instance, params object[] parameters)
        {
            try
            {
                var value = _method.Invoke(instance, parameters ?? NoParams);

                if (value == null)
                    return default;

                return (RT)value;
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Exception while calling {nameof(MethodAccessor<T, RT>)} method \"{Identifier}\"!");
                ArchiveLogger.Exception(ex);
            }
            return default;
        }

        /// <summary>
        /// Invoke the reflected method on an <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An object instance to invoke the method on (leave null if method is static)</param>
        /// <returns></returns>
        public RT Invoke(T instance) => Invoke(instance, null);
    }

    /// <summary>
    /// Globally cached reflection wrapper for void methods.
    /// </summary>
    /// <typeparam name="T">The Type that the method belongs to</typeparam>
    public class MethodAccessor<T> : AccessorBase
    {
        /// <summary>
        /// Gets a <see cref="MethodAccessor{T}"/> from the global cache or creates a new instance if there is none and adds it to the cache.
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <param name="parameterTypes">Parameter Types of the method (leave null if there are none)</param>
        /// <returns><see cref="MethodAccessor{T}"/></returns>
        public static MethodAccessor<T> GetAccessor(string methodName, Type[] parameterTypes = null)
        {
            var identifier = $"Method_{typeof(T).FullName}_void_{methodName}";
            if (parameterTypes != null)
            {
                identifier += $"_{string.Join("_", parameterTypes.Select(pt => pt.Name))}";
            }

            if (Accessors.TryGetValue(identifier, out var val))
                return (MethodAccessor<T>)val;

            val = new MethodAccessor<T>(identifier, methodName, parameterTypes);

            Accessors.Add(identifier, val);

            return (MethodAccessor<T>)val;
        }

        private readonly MethodInfo _method;
        public bool IsMethodStatic => _method.IsStatic;
        public override bool HasMemberBeenFound => _method != null;

        private MethodAccessor(string identifier, string methodName, Type[] parameterTypes) : base(identifier)
        {
            try
            {
                if(parameterTypes == null)
                {
                    _method = typeof(T).GetMethod(methodName, AnyBindingFlags);
                }
                else
                {
                    _method = typeof(T).GetMethod(methodName, AnyBindingFlags, null, parameterTypes, null);
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Method \"{methodName}\" in Type {typeof(T).FullName} could not be resolved on {ex.Source}!");
                ArchiveLogger.Exception(ex);
            }
        }

        /// <summary>
        /// Invoke the reflected method on an <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An object instance to invoke the method on (leave null if method is static)</param>
        /// <param name="parameters">(Optional) method parameters</param>
        public void Invoke(T instance, params object[] parameters)
        {
            try
            {
                _method.Invoke(instance, parameters ?? NoParams);
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Exception while calling {nameof(MethodAccessor<T>)} method \"{Identifier}\"!");
                ArchiveLogger.Exception(ex);
            }
        }

        /// <summary>
        /// Invoke the reflected method on an <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An object instance to invoke the method on (leave null if method is static)</param>
        public void Invoke(T instance) => Invoke(instance, null);
    }
}
