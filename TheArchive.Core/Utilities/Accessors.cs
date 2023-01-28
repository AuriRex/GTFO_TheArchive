using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TheArchive.Utilities
{
    /// <summary>
    /// Globally cached reflection wrapper for fields or properties.
    /// </summary>
    /// <typeparam name="T">The Type that the member belongs to</typeparam>
    /// <typeparam name="MT">The Type of the member itself</typeparam>
    public interface IValueAccessor<T, MT>
    {
        /// <summary>
        /// If setting a value is possible
        /// </summary>
        public bool CanGet { get; }

        /// <summary>
        /// If getting a value is possible
        /// </summary>
        public bool CanSet { get; }

        /// <summary>
        /// Get the value of the reflected member from an <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An object instance to get the value from</param>
        /// <returns></returns>
        public MT Get(T instance);

        /// <summary>
        /// Set the <paramref name="value"/> of the reflected member on an <paramref name="instance"/>. 
        /// </summary>
        /// <param name="instance">An object instance to set the value of</param>
        /// <param name="value">The new value</param>
        public void Set(T instance, MT value);
    }

    /// <summary>
    /// Globally cached reflection wrapper for fields or properties (static edition!).
    /// </summary>
    /// <typeparam name="T">The Type that the member belongs to</typeparam>
    /// <typeparam name="MT">The Type of the member itself</typeparam>
    public interface IStaticValueAccessor<T, MT> : IValueAccessor<T, MT>
    {
        /// <summary>
        /// If the reflected member is static.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// Get the value of the reflected static member
        /// </summary>
        /// <returns></returns>
        public MT GetStaticValue();

        /// <summary>
        /// Set the <paramref name="value"/> of the reflected member on a static member
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetStaticValue(MT value);
    }

    /// <summary>
    /// AccessorBase, contains a static Dictionary containing all cached accessors.
    /// </summary>
    public abstract class AccessorBase
    {
        public static object[] NoParams { get; private set; } = Array.Empty<object>();
        public static BindingFlags AnyBindingFlags => Utils.AnyBindingFlagss;

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

        /// <summary>
        /// Finds and creates a <see cref="IValueAccessor{T, MT}"/> given a <paramref name="memberName"/>.
        /// <br/><br/>
        /// Handles both fields and properties on mono games as well as IL2CPP
        /// </summary>
        /// <typeparam name="T">The Type that the member belongs to</typeparam>
        /// <typeparam name="MT">The Type of the member itself</typeparam>
        /// <param name="memberName"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IValueAccessor<T, MT> GetValueAccessor<T, MT>(string memberName, bool throwOnError = false)
        {
            if(Loader.LoaderWrapper.IsGameIL2CPP() && Loader.LoaderWrapper.IsIL2CPPType(typeof(T)))
            {
                // All fields are turned into properties by unhollower/interop!
                return PropertyAccessor<T, MT>.GetAccessor(memberName);
            }

            var member = typeof(T).GetMember(memberName, Utils.AnyBindingFlagss).FirstOrDefault();

            switch(member)
            {
                case PropertyInfo:
                    return PropertyAccessor<T, MT>.GetAccessor(memberName);
                case FieldInfo:
                    return FieldAccessor<T, MT>.GetAccessor(memberName);
            }

            if (throwOnError)
                throw new ArgumentException($"Member with name \"{memberName}\" could not be found in type \"{typeof(T).Name}\" or isn't {nameof(IValueAccessor<T, MT>)} compatible.", nameof(memberName));
            
            return null;
        }

        /// <summary>
        /// Finds and creates a <see cref="IValueAccessor{T, MT}"/> given a <paramref name="memberName"/> (static edition!).
        /// <br/><br/>
        /// Handles both fields and properties on mono games as well as IL2CPP
        /// </summary>
        /// <typeparam name="T">The Type that the member belongs to</typeparam>
        /// <typeparam name="MT">The Type of the member itself</typeparam>
        /// <param name="memberName"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IStaticValueAccessor<T, MT> GetStaticValueAccessor<T, MT>(string memberName, bool throwOnError = false)
        {
            var accessor = GetValueAccessor<T, MT>(memberName, throwOnError);

            if(accessor != null)
            {
                var staticAccessor = accessor as IStaticValueAccessor<T, MT>;

                if (throwOnError && !staticAccessor.IsStatic)
                    throw new ArgumentException($"Member with name \"{memberName}\" is not static!", nameof(memberName));

                return staticAccessor;
            }

            return null;
        }
    }

    /// <summary>
    /// Globally cached reflection wrapper for fields.
    /// </summary>
    /// <typeparam name="T">The Type that the field belongs to</typeparam>
    /// <typeparam name="FT">The Type of the field itself</typeparam>
    public class FieldAccessor<T, FT> : AccessorBase, IValueAccessor<T, FT>, IStaticValueAccessor<T, FT>
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

        public bool CanGet => true;

        public bool CanSet => true;

        public bool IsStatic => _field?.IsStatic ?? false;

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

        public FT GetStaticValue()
        {
            return Get(default);
        }

        public void SetStaticValue(FT value)
        {
            Set(default, value);
        }
    }

    /// <summary>
    /// Globally cached reflection wrapper for properties.
    /// </summary>
    /// <typeparam name="T">The Type that the property belongs to</typeparam>
    /// <typeparam name="PT">The Type of the property itself</typeparam>
    public class PropertyAccessor<T, PT> : AccessorBase, IValueAccessor<T, PT>, IStaticValueAccessor<T, PT>
    {
        /// <summary>
        /// Gets a <see cref="PropertyAccessor{T, FT}"/> from the global cache or creates a new instance if there is none and adds it to the cache.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns><see cref="PropertyAccessor{T, FT}"/></returns>
        public static PropertyAccessor<T, PT> GetAccessor(string propertyName)
        {
            var identifier = $"Property_{typeof(T).FullName}_{propertyName}";

            if (Accessors.TryGetValue(identifier, out var val))
                return (PropertyAccessor<T, PT>)val;

            val = new PropertyAccessor<T, PT>(identifier, propertyName);

            Accessors.Add(identifier, val);

            return (PropertyAccessor<T, PT>)val;
        }

        private readonly PropertyInfo _property;

        public override bool HasMemberBeenFound => _property != null;

        public bool CanGet => _property?.GetGetMethod(true) != null;

        public bool CanSet => _property?.GetSetMethod(true) != null;

        public bool IsStatic => (_property?.GetGetMethod(true) ?? _property?.GetSetMethod(true))?.IsStatic ?? false;

        private PropertyAccessor(string identifier, string propertyName) : base(identifier)
        {
            _property = typeof(T).GetProperty(propertyName, AnyBindingFlags);
        }

        /// <summary>
        /// Get the value of the reflected property from an <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An object instance to get the value from</param>
        /// <returns></returns>
        public PT Get(T instance)
        {
            try
            {
                return (PT)_property.GetValue(instance);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Exception while getting {nameof(FieldAccessor<T, PT>)} property \"{Identifier}\"!");
                ArchiveLogger.Exception(ex);
            }
            return default;
        }

        /// <summary>
        /// Set the <paramref name="value"/> of the reflected property on an <paramref name="instance"/>. 
        /// </summary>
        /// <param name="instance">An object instance to set the value of</param>
        /// <param name="value">The new value</param>
        public void Set(T instance, PT value)
        {
            try
            {
                _property.SetValue(instance, value);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Exception while setting {nameof(FieldAccessor<T, PT>)} property \"{Identifier}\"!");
                ArchiveLogger.Exception(ex);
            }
        }

        public PT GetStaticValue()
        {
            return Get(default);
        }

        public void SetStaticValue(PT value)
        {
            Set(default, value);
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
        public static MethodAccessor<T> GetAccessor(string methodName, Type[] parameterTypes = null, bool ignoreErrors = false)
        {
            var identifier = $"Method_{typeof(T).FullName}_void_{methodName}";
            if (parameterTypes != null && parameterTypes != Array.Empty<Type>())
            {
                identifier += $"_{string.Join("_", parameterTypes.Select(pt => pt.Name))}";
            }

            if (Accessors.TryGetValue(identifier, out var val))
                return (MethodAccessor<T>)val;

            val = new MethodAccessor<T>(identifier, methodName, parameterTypes, ignoreErrors);

            Accessors.Add(identifier, val);

            return (MethodAccessor<T>)val;
        }

        private readonly MethodInfo _method;
        public bool IsMethodStatic => _method.IsStatic;
        public override bool HasMemberBeenFound => _method != null;
        public int ParameterCount { get; private set; }

        private MethodAccessor(string identifier, string methodName, Type[] parameterTypes, bool ignoreErrors = false) : base(identifier)
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
                if (!ignoreErrors && _method == null) throw new Exception("Method not found!");

                if(_method != null)
                {
                    ParameterCount = _method.GetParameters().Length;
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Method \"{methodName}\" in Type {typeof(T).FullName} could not be resolved on {ex.Source}!");
                ArchiveLogger.Exception(ex);
                ArchiveLogger.Debug($"Constructor debug data:\nidentifier:{identifier}\nmethodName:{methodName}\nparameterTypes:{string.Join(", ", parameterTypes.Select(p => p.FullName))}");
                var frame = new System.Diagnostics.StackTrace().GetFrame(2);
                ArchiveLogger.Debug($"FileName:{frame.GetFileName()} {frame.GetFileLineNumber()}\nMethod:{frame.GetMethod().DeclaringType.FullName}:{frame.GetMethod().Name}");
                PrintDebug();
            }
        }

        private void PrintDebug()
        {
            if (_method == null) return;
            ArchiveLogger.Debug($"Method debug data:\nName:{_method.Name}\nDeclaringType:{_method.DeclaringType.FullName}\nReturnType:{_method.ReturnType}\nParameter Count:{_method.GetParameters()?.Count() ?? 0}\nParameters:{string.Join(", ", _method.GetParameters().Select(p => p.ParameterType.FullName))}");
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
                if(parameters != null && parameters.Length > ParameterCount)
                {
                    parameters = parameters.Take(ParameterCount).ToArray();
                }
                _method.Invoke(instance, parameters ?? NoParams);
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Exception while calling {nameof(MethodAccessor<T>)} method \"{Identifier}\"!");
                ArchiveLogger.Exception(ex);
                PrintDebug();
            }
        }

        /// <summary>
        /// Invoke the reflected method on an <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An object instance to invoke the method on (leave null if method is static)</param>
        public void Invoke(T instance) => Invoke(instance, null);
    }
}
