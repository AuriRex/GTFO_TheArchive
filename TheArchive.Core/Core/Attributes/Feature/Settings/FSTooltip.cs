using CellMenu;
using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Set the tooltip of a feature setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FSTooltip : Localized
{
    /// <summary>
    /// The feature settings tooltip position type.
    /// </summary>
    public TooltipPositionType PositionType { get; private set; }
    /// <summary>
    /// The feature settings tooltip header.
    /// </summary>
    public string TooltipHeader { get; private set; }

    /// <summary>
    /// The feature settings tooltip text.
    /// </summary>
    public string TooltipText { get; private set; }

    /// <summary>
    /// Set the tooltip of a feature setting.
    /// </summary>
    /// <param name="header">The tooltip header of the feature setting.</param>
    /// <param name="text">The tooltip text of the feature setting.</param>
    /// <param name="positionType">The tooltip position type of the feature setting.</param>
    public FSTooltip(string text, string header = null, TooltipPositionType positionType = TooltipPositionType.UnderElement) : base()
    {
        TooltipHeader = header;
        TooltipText = text;
        PositionType = positionType;
    }
}
