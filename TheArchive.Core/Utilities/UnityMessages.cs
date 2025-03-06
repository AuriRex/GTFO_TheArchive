using System.Diagnostics.CodeAnalysis;

namespace TheArchive.Utilities;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class UnityMessages
{
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
}