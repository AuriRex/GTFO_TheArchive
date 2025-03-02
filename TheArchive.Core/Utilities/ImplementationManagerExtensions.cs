using System;
using TheArchive.Core.Managers;

namespace TheArchive.Utilities;

public static class ImplementationManagerExtensions
{

    public static void RegisterForIdentifier(this Type type, string identifier)
    {
        ImplementationManager.RegisterGameType(identifier, type);
    }

    public static void RegisterSelf<T>(this T type) where T : Type
    {
        if(type.IsGenericTypeDefinition)
        {
            RegisterForIdentifier(type, $"{type.Name.Split('`')[0]}<{(type.GenericTypeArguments.Length > 1 ? string.Join(",", new string[type.GenericTypeArguments.Length-1]) : string.Empty)}>");
        }
        else
        {
            RegisterForIdentifier(type, type.Name);
        }
    }

}