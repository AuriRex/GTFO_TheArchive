using System;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Utilities;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting that handles all the different kind of numbers, from byte to double.
/// </summary>
public class NumberSetting : FeatureSetting
{
    /// <summary>
    /// Do we have a slider?
    /// </summary>
    public bool HasSlider => Slider != null;
    
    /// <summary>
    /// Slider to use
    /// </summary>
    public FSSlider Slider { get; }
    
    /// <summary>
    /// Timestamp format to use
    /// </summary>
    public FSTimestamp Timestamp { get; }
    
    /// <summary>
    /// The type of the reflected number.<br/>
    /// Depends on the properties type.
    /// </summary>
    /// <seealso cref="NumberFormat"/>
    public NumberFormat Format { get; }

    /// <inheritdoc/>
    public NumberSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        Timestamp = prop.GetCustomAttribute<FSTimestamp>();
        Slider = prop.GetCustomAttribute<FSSlider>();

        switch (Type.Name)
        {
            case nameof(Int64):
                Format = NumberFormat.Int64;
                break;
            case nameof(Int32):
                Format = NumberFormat.Int32;
                break;
            case nameof(Int16):
                Format = NumberFormat.Int16;
                break;
            case nameof(UInt64):
                Format = NumberFormat.UInt64;
                break;
            case nameof(UInt32):
                Format = NumberFormat.UInt32;
                break;
            case nameof(UInt16):
                Format = NumberFormat.UInt16;
                break;
            case nameof(Byte):
                Format = NumberFormat.Byte;
                break;
            case nameof(SByte):
                Format = NumberFormat.SByte;
                break;
            case nameof(Single):
                Format = NumberFormat.Single;
                break;
            case nameof(Double):
                Format = NumberFormat.Double;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert a number to the selected NumberFormat.
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <returns>The value converted to NumberFormat</returns>
    /// <seealso cref="Format"/>
    public object ConvertNumber(object value)
    {
        switch(Format)
        {
            case NumberFormat.Single:
                return Convert.ToSingle(value);
            case NumberFormat.Double:
                return Convert.ToDouble(value);
            case NumberFormat.Byte:
                return Convert.ToByte(value);
            case NumberFormat.UInt16:
                return Convert.ToUInt16(value);
            case NumberFormat.UInt32:
                return Convert.ToUInt32(value);
            case NumberFormat.UInt64:
                return Convert.ToUInt64(value);
            case NumberFormat.SByte:
                return Convert.ToSByte(value);
            case NumberFormat.Int16:
                return Convert.ToInt16(value);
            case NumberFormat.Int32:
                return Convert.ToInt32(value);
            default:
            case NumberFormat.Int64:
                return Convert.ToInt64(value);
        }
    }

    /// <inheritdoc/>
    public override object GetValue()
    {
        var value = base.GetValue();

        if(Timestamp != null && long.TryParse(value.ToString(), out long ticks))
        {
            value = new DateTime(ticks).ToString(Timestamp.Format);
        }

        return value;
    }

    /// <inheritdoc/>
    public override object SetValue(object value)
    {
        if(Timestamp != null)
        {
            ArchiveLogger.Warning($"Can't set backing value of {nameof(NumberSetting)} \"{DEBUG_Path}\" with {nameof(FSTimestamp)} attribute!");
            return value;
        }

        var val = value.ToString();
        if(string.IsNullOrWhiteSpace(val))
        {
            val = "0";
        }
        try
        {
            val = base.SetValue(ConvertNumber(val)).ToString();
        }
        catch (FormatException ex)
        {
            Helper.Feature.FeatureLogger.Warning($"[{nameof(NumberSetting)}] {ex.GetType().FullName} was thrown! Invalid input data \"{value}\".");
            val = "0";
        }
        return val;
    }

    /// <summary>
    /// The type/format of a number setting.
    /// </summary>
    public enum NumberFormat
    {
        /// <summary> <see cref="sbyte"/> </summary>
        SByte,
        /// <summary> <see cref="Int16"/> </summary>
        Int16,
        /// <summary> <see cref="Int32"/> </summary>
        Int32,
        /// <summary> <see cref="Int64"/> </summary>
        Int64,
        /// <summary> <see cref="byte"/> </summary>
        Byte,
        /// <summary> <see cref="UInt16"/> </summary>
        UInt16,
        /// <summary> <see cref="UInt32"/> </summary>
        UInt32,
        /// <summary> <see cref="UInt64"/> </summary>
        UInt64,
        /// <summary> <see cref="Single"/> </summary>
        Single,
        /// <summary> <see cref="Double"/> </summary>
        Double,
    }
}