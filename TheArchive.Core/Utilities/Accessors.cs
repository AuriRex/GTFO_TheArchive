using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace TheArchive.Utilities;

/// <summary>
/// Accessor related extension methods.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class AccessorExtensions
{
    /// <summary>
    /// Provides an alternative source for an accessor in case the primary one is not valid.
    /// </summary>
    /// <param name="self">The primary accessor.</param>
    /// <param name="func">A function providing a replacement in case the primary did not find its target.</param>
    /// <typeparam name="T">Type</typeparam>
    /// <typeparam name="MT">Member type.</typeparam>
    /// <returns>The primary accessor if it's valid, else the fallback one provided by <paramref name="func"/>.</returns>
    /// <exception cref="NullReferenceException"><paramref name="func"/> must not be null.</exception>
    public static IValueAccessor<T, MT> OrAlternative<T, MT>(this IValueAccessor<T, MT> self, Func<IValueAccessor<T, MT>> func)
    {
        if (self != null && self.HasMember)
            return self;

        if (func == null)
            throw new NullReferenceException($"Parameter {nameof(func)} may not be null!");

        return func.Invoke();
    }
}

/// <summary>
/// Globally cached reflection wrapper for fields or properties.
/// </summary>
/// <typeparam name="T">The Type that the member belongs to</typeparam>
/// <typeparam name="MT">The Type of the member itself</typeparam>
[SuppressMessage("ReSharper", "InconsistentNaming")]
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
    /// If the member has been found
    /// </summary>
    public bool HasMember { get; }

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
[SuppressMessage("ReSharper", "InconsistentNaming")]
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class AccessorBase
{
    /// <summary>
    /// An empty object array.
    /// </summary>
    protected static object[] NoParams { get; } = Array.Empty<object>();
    
    /// <summary>
    /// Any binding flags.
    /// </summary>
    protected static BindingFlags AnyBindingFlags => Utils.AnyBindingFlagss;
    
    /// <summary>
    /// Cache of already existing accessors.
    /// </summary>
    protected static readonly Dictionary<string, AccessorBase> Accessors = new();

    /// <summary>
    /// Identifies the reflected member.<br/>
    /// </summary>
    public string Identifier { get; private set; }

    /// <summary>
    /// Should errors be ignored?
    /// </summary>
    public bool IgnoreErrors { get; private set; }

    /// <summary>
    /// If the reflected member has been found.
    /// </summary>
    public abstract bool HasMemberBeenFound { get; }

    /// <summary>
    /// AccessorBase constructor.
    /// </summary>
    /// <param name="identifier">This accessors identifier - used for caching.</param>
    /// <param name="ignoreErrors">Should errors be ignored?</param>
    protected AccessorBase(string identifier, bool ignoreErrors)
    {
        Identifier = identifier;
        IgnoreErrors = ignoreErrors;
    }

    /// <summary>
    /// Finds and creates a <see cref="IValueAccessor{T, MT}"/> given a <paramref name="memberName"/>.
    /// <br/><br/>
    /// Handles both fields and properties on mono games as well as IL2CPP
    /// </summary>
    /// <typeparam name="T">The Type that the member belongs to</typeparam>
    /// <typeparam name="MT">The Type of the member itself</typeparam>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="throwOnError">Should an exception be thrown on error?</param>
    /// <returns>The value accessor for a given member.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static IValueAccessor<T, MT> GetValueAccessor<T, MT>(string memberName, bool throwOnError = false)
    {
        if(Loader.LoaderWrapper.IsGameIL2CPP() && Loader.LoaderWrapper.IsIL2CPPType(typeof(T)))
        {
            // All fields are turned into properties by unhollower/interop!
            return PropertyAccessor<T, MT>.GetAccessor(memberName, !throwOnError);
        }

        var member = typeof(T).GetMember(memberName, Utils.AnyBindingFlagss).FirstOrDefault();

        switch(member)
        {
            case PropertyInfo:
                return PropertyAccessor<T, MT>.GetAccessor(memberName, !throwOnError);
            case FieldInfo:
                return FieldAccessor<T, MT>.GetAccessor(memberName, !throwOnError);
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
    /// <param name="memberName">The name of the member.</param>
    /// <param name="throwOnError">Should an exception be thrown on error?</param>
    /// <returns>The value accessor for a given member.</returns>
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FieldAccessor<T, FT> : AccessorBase, IValueAccessor<T, FT>, IStaticValueAccessor<T, FT>
{
    /// <summary>
    /// Gets a <see cref="FieldAccessor{T, FT}"/> from the global cache or creates a new instance if there is none and adds it to the cache.
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="ignoreErrors">If Exceptions should be ignored</param>
    /// <returns><see cref="FieldAccessor{T, FT}"/></returns>
    public static FieldAccessor<T, FT> GetAccessor(string fieldName, bool ignoreErrors = false)
    {
        var identifier = $"Field_{typeof(T).FullName}_{fieldName}";

        if (Accessors.TryGetValue(identifier, out var val))
            return (FieldAccessor<T, FT>) val;

        val = new FieldAccessor<T, FT>(identifier, fieldName, ignoreErrors);

        Accessors.Add(identifier, val);

        return (FieldAccessor<T, FT>) val;
    }

    private readonly FieldInfo _field;

    /// <inheritdoc/>
    public override bool HasMemberBeenFound => _field != null;

    /// <inheritdoc/>
    public bool CanGet => true;

    /// <inheritdoc/>
    public bool CanSet => true;

    /// <inheritdoc/>
    public bool HasMember => HasMemberBeenFound;

    /// <inheritdoc/>
    public bool IsStatic => _field?.IsStatic ?? false;

    /// <summary>
    /// Creates a new instance of a FieldAccessor.
    /// </summary>
    /// <param name="identifier">This accessors identifier - used for caching.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="ignoreErrors">Should errors be ignored?</param>
    private FieldAccessor(string identifier, string fieldName, bool ignoreErrors = false) : base(identifier, ignoreErrors)
    {
        _field = typeof(T).GetField(fieldName, AnyBindingFlags);
    }

    /// <summary>
    /// Get the value of the reflected field from an <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">An object instance to get the value from.</param>
    /// <returns>The value of the field.</returns>
    public FT Get(T instance)
    {
        try
        {
            return (FT)_field.GetValue(instance);
        }
        catch (NullReferenceException)
        {
            if (!IgnoreErrors)
            {
                if (!HasMemberBeenFound)
                {
                    ArchiveLogger.Warning($"NullReferenceException while getting {nameof(FieldAccessor<T, FT>)} field \"{Identifier}\"! If this is intentional consider setting {nameof(IgnoreErrors)} to true.");
                    return default;
                }

                ArchiveLogger.Error($"NullReferenceException while getting {nameof(FieldAccessor<T, FT>)} field \"{Identifier}\"!");
                throw;
            }
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error($"Exception while getting {nameof(FieldAccessor<T, FT>)} field \"{Identifier}\"!");
            ArchiveLogger.Exception(ex);
        }
        return default;
    }

    /// <summary>
    /// Set the <paramref name="value"/> of the reflected field on an <paramref name="instance"/>. 
    /// </summary>
    /// <param name="instance">An object instance to set the value of.</param>
    /// <param name="value">The new value.</param>
    public void Set(T instance, FT value)
    {
        try
        {
            _field.SetValue(instance, value);
        }
        catch (NullReferenceException)
        {
            if (!IgnoreErrors)
            {
                if (!HasMemberBeenFound)
                {
                    ArchiveLogger.Warning($"NullReferenceException while setting {nameof(FieldAccessor<T, FT>)} field \"{Identifier}\"! If this is intentional consider setting {nameof(IgnoreErrors)} to true.");
                    return;
                }

                ArchiveLogger.Error($"NullReferenceException while setting {nameof(FieldAccessor<T, FT>)} field \"{Identifier}\"!");
                throw;
            }
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error($"Exception while setting {nameof(FieldAccessor<T, FT>)} field \"{Identifier}\"!");
            ArchiveLogger.Exception(ex);
        }
    }

    /// <summary>
    /// Get the value of a static field.
    /// </summary>
    /// <returns>The fields value.</returns>
    public FT GetStaticValue()
    {
        return Get(default);
    }

    /// <summary>
    /// Set the value of a static field.
    /// </summary>
    /// <param name="value">The new value.</param>
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class PropertyAccessor<T, PT> : AccessorBase, IValueAccessor<T, PT>, IStaticValueAccessor<T, PT>
{
    /// <summary>
    /// Gets a <see cref="PropertyAccessor{T, FT}"/> from the global cache or creates a new instance if there is none and adds it to the cache.
    /// </summary>
    /// <param name="propertyName">The name of the property</param>
    /// <param name="ignoreErrors">If Exceptions should be ignored</param>
    /// <returns><see cref="PropertyAccessor{T, FT}"/></returns>
    public static PropertyAccessor<T, PT> GetAccessor(string propertyName, bool ignoreErrors = false)
    {
        var identifier = $"Property_{typeof(T).FullName}_{propertyName}";

        if (Accessors.TryGetValue(identifier, out var val))
            return (PropertyAccessor<T, PT>)val;

        val = new PropertyAccessor<T, PT>(identifier, propertyName, ignoreErrors);

        Accessors.Add(identifier, val);

        return (PropertyAccessor<T, PT>)val;
    }

    private readonly PropertyInfo _property;

    /// <inheritdoc/>
    public override bool HasMemberBeenFound => _property != null;

    /// <inheritdoc/>
    public bool CanGet => _property?.GetGetMethod(true) != null;

    /// <inheritdoc/>
    public bool CanSet => _property?.GetSetMethod(true) != null;

    /// <inheritdoc/>
    public bool HasMember => HasMemberBeenFound;

    /// <inheritdoc/>
    public bool IsStatic => (_property?.GetGetMethod(true) ?? _property?.GetSetMethod(true))?.IsStatic ?? false;
    
    /// <summary>
    /// Creates a new instance of a PropertyAccessor.
    /// </summary>
    /// <param name="identifier">This accessors identifier - used for caching.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="ignoreErrors">Should errors be ignored?</param>
    private PropertyAccessor(string identifier, string propertyName, bool ignoreErrors = false) : base(identifier, ignoreErrors)
    {
        _property = typeof(T).GetProperty(propertyName, AnyBindingFlags);
    }

    /// <summary>
    /// Get the value of the reflected property from an <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">An object instance to get the value from.</param>
    /// <returns>The properties value.</returns>
    public PT Get(T instance)
    {
        try
        {
            return (PT)_property.GetValue(instance);
        }
        catch (NullReferenceException)
        {
            if (!IgnoreErrors)
            {
                if (!HasMemberBeenFound)
                {
                    ArchiveLogger.Warning($"NullReferenceException while getting {nameof(PropertyAccessor<T, PT>)} property \"{Identifier}\"! If this is intentional consider setting {nameof(IgnoreErrors)} to true.");
                    return default;
                }

                ArchiveLogger.Error($"NullReferenceException while getting {nameof(PropertyAccessor<T, PT>)} property \"{Identifier}\"!");
                throw;
            }
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error($"Exception while getting {nameof(PropertyAccessor<T, PT>)} property \"{Identifier}\"!");
            ArchiveLogger.Exception(ex);
        }
        return default;
    }

    /// <summary>
    /// Set the <paramref name="value"/> of the reflected property on an <paramref name="instance"/>. 
    /// </summary>
    /// <param name="instance">An object instance to set the value of.</param>
    /// <param name="value">The new value.</param>
    public void Set(T instance, PT value)
    {
        try
        {
            _property.SetValue(instance, value);
        }
        catch (NullReferenceException)
        {
            if (!IgnoreErrors)
            {
                if (!HasMemberBeenFound)
                {
                    ArchiveLogger.Warning($"NullReferenceException while setting {nameof(PropertyAccessor<T, PT>)} property \"{Identifier}\"! If this is intentional consider setting {nameof(IgnoreErrors)} to true.");
                    return;
                }

                ArchiveLogger.Error($"NullReferenceException while setting {nameof(PropertyAccessor<T, PT>)} property \"{Identifier}\"!");
                throw;
            }
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error($"Exception while setting {nameof(PropertyAccessor<T, PT>)} property \"{Identifier}\"!");
            ArchiveLogger.Exception(ex);
        }
    }

    /// <summary>
    /// Get a static properties value.
    /// </summary>
    /// <returns>The properties value.</returns>
    public PT GetStaticValue()
    {
        return Get(default);
    }

    /// <summary>
    /// Set a static properties value.
    /// </summary>
    /// <param name="value">The new value.</param>
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class MethodAccessor<T, RT> : AccessorBase
{
    /// <summary>
    /// Gets a <see cref="MethodAccessor{T, RT}"/> from the global cache or creates a new instance if there is none and adds it to the cache.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="parameterTypes">Parameter Types of the method (leave null if there are none).</param>
    /// <param name="ignoreErrors">Should errors be ignored?</param>
    /// <returns><see cref="MethodAccessor{T, RT}"/></returns>
    public static MethodAccessor<T, RT> GetAccessor(string methodName, Type[] parameterTypes = null, bool ignoreErrors = false)
    {
        var identifier = $"Method_{typeof(T).FullName}_{typeof(RT)}_{methodName}";
        if(parameterTypes != null)
        {
            identifier += $"_{string.Join("_", parameterTypes.Select(pt => pt.Name))}";
        }

        if (Accessors.TryGetValue(identifier, out var val))
            return (MethodAccessor<T, RT>) val;

        val = new MethodAccessor<T, RT>(identifier, methodName, parameterTypes, ignoreErrors);

        Accessors.Add(identifier, val);

        return (MethodAccessor<T, RT>) val;
    }

    private readonly MethodInfo _method;
    
    /// <summary>
    /// Is the reflected method static?
    /// </summary>
    public bool IsMethodStatic => _method.IsStatic;
    
    /// <inheritdoc/>
    public override bool HasMemberBeenFound => _method != null;

    /// <summary>
    /// Creates a new instance of a MethodAccessor.
    /// </summary>
    /// <param name="identifier">This accessors identifier - used for caching.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="parameterTypes">Parameter types for distinguishing between overloads. (this may be null)</param>
    /// <param name="ignoreErrors">Should errors be ignored?</param>
    private MethodAccessor(string identifier, string methodName, Type[] parameterTypes, bool ignoreErrors = false) : base(identifier, ignoreErrors)
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
        catch (NullReferenceException)
        {
            if (!IgnoreErrors)
            {
                if (!HasMemberBeenFound)
                {
                    ArchiveLogger.Warning($"NullReferenceException while calling {nameof(MethodAccessor<T, RT>)} method \"{Identifier}\"! If this is intentional consider setting {nameof(IgnoreErrors)} to true.");
                    return default;
                }

                ArchiveLogger.Error($"NullReferenceException while calling {nameof(MethodAccessor<T, RT>)} method \"{Identifier}\"!");
                throw;
            }
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
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class MethodAccessor<T> : AccessorBase
{
    /// <summary>
    /// Gets a <see cref="MethodAccessor{T}"/> from the global cache or creates a new instance if there is none and adds it to the cache.
    /// </summary>
    /// <param name="methodName">The name of the method</param>
    /// <param name="parameterTypes">Parameter Types of the method (leave null if there are none).</param>
    /// <param name="ignoreErrors">Should errors be ignored?</param>
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
    
    /// <inheritdoc cref="MethodAccessor{T,RT}.IsMethodStatic"/>
    public bool IsMethodStatic => _method.IsStatic;
    
    /// <inheritdoc cref="MethodAccessor{T,RT}.HasMemberBeenFound"/>
    public override bool HasMemberBeenFound => _method != null;
    
    /// <summary>
    /// The parameter count of the reflected method.
    /// </summary>
    public int ParameterCount { get; private set; }

    /// <summary>
    /// Creates a new instance of a MethodAccessor.
    /// </summary>
    /// <param name="identifier">This accessors identifier - used for caching.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="parameterTypes">Parameter types for distinguishing between overloads. (this may be null)</param>
    /// <param name="ignoreErrors">Should errors be ignored?</param>
    /// <exception cref="Exception"></exception>
    private MethodAccessor(string identifier, string methodName, Type[] parameterTypes, bool ignoreErrors = false) : base(identifier, ignoreErrors)
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
            parameterTypes ??= Array.Empty<Type>();
            ArchiveLogger.Error($"Method \"{methodName}\" in Type {typeof(T).FullName} could not be resolved on {ex.Source}!");
            ArchiveLogger.Exception(ex);
            ArchiveLogger.Debug($"Constructor debug data:\nidentifier:{identifier}\nmethodName:{methodName}\nparameterTypes:{string.Join(", ", parameterTypes.Select(p => p.FullName))}");
            var frame = new System.Diagnostics.StackTrace().GetFrame(2)!;
            ArchiveLogger.Debug($"FileName:{frame.GetFileName()} {frame.GetFileLineNumber()}\nMethod:{frame.GetMethod()?.DeclaringType?.FullName ?? "Unknown"}:{frame.GetMethod()?.Name ?? "Unknown"}");
            PrintDebug();
        }
    }

    private void PrintDebug()
    {
        if (_method == null) return;
        ArchiveLogger.Debug($"Method debug data:\nName:{_method.Name}\nDeclaringType:{_method.DeclaringType?.FullName ?? "Unknown"}\nReturnType:{_method.ReturnType}\nParameter Count:{_method.GetParameters().Length}\nParameters:{string.Join(", ", _method.GetParameters().Select(p => p.ParameterType.FullName))}");
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
        catch(NullReferenceException)
        {
            if(!IgnoreErrors)
            {
                if(!HasMemberBeenFound)
                {
                    ArchiveLogger.Warning($"NullReferenceException while calling {nameof(MethodAccessor<T>)} method \"{Identifier}\"! If this is intentional consider setting {nameof(IgnoreErrors)} to true.");
                    return;
                }

                ArchiveLogger.Error($"NullReferenceException while calling {nameof(MethodAccessor<T>)} method \"{Identifier}\"!");
                throw;
            }
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