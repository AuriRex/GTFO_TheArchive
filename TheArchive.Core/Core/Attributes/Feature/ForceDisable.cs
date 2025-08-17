using System;

namespace TheArchive.Core.Attributes.Feature;

/// <summary>
/// Forces this <c>Feature</c> to be disabled.<br/>
/// It can not be enabled by any means in game.
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate your <c>Feature</c> types.</item>
/// </list>
/// </remarks>
/// <seealso cref="FeaturesAPI.Feature"/>
/// <example><code>
/// [ForceDisable("Reason as to why.")]
/// public class MyFeature : Feature
/// {
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class)]
public class ForceDisable : Attribute
{
    /// <summary>
    /// The reason as to why this <c>Feature</c> should be forcefully disabled.
    /// </summary>
    public string Justification { get; set; }
    
    /// <summary>
    /// Forces this <c>Feature</c> to be disabled.<br/>
    /// It can not be enabled by any means in game.
    /// </summary>
    /// <param name="justification">The reason as to why this <c>Feature</c> should be forcefully disabled.</param>
    public ForceDisable(string justification)
    {
        Justification = justification;
    }
}