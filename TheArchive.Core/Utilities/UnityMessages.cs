using System.Diagnostics.CodeAnalysis;

namespace TheArchive.Utilities;

/// <summary>
/// Constants of unity message names.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class UnityMessages
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string Awake = "Awake";
    public const string Start = "Start";
    public const string Update = "Update";
    public const string FixedUpdate = "FixedUpdate";
    public const string LateUpdate = "LateUpdate";
    public const string OnEnable = "OnEnable";
    public const string OnDisable = "OnDisable";
    public const string OnDestroy = "OnDestroy";
    public const string OnApplicationFocus = "OnApplicationFocus";
    public const string OnApplicationQuit = "OnApplicationQuit";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}