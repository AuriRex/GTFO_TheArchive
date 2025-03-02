using System;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Utilities;

namespace TheArchive.Core.FeaturesAPI.Settings;

public class NumberSetting : FeatureSetting
{
    public bool HasSlider => Slider != null;
    public FSSlider Slider { get; private set; } = null;
    public FSTimestamp Timestamp { get; private set; } = null;
    public NumberFormat Format { get; private set; }

    public NumberSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
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

    public override object GetValue()
    {
        var value = base.GetValue();

        if(Timestamp != null && long.TryParse(value.ToString(), out long ticks))
        {
            value = new DateTime(ticks).ToString(Timestamp.Format);
        }

        return value;
    }

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

    public enum NumberFormat
    {
        SByte,
        Int16,
        Int32,
        Int64,
        Byte,
        UInt16,
        UInt32,
        UInt64,
        Single,
        Double,
    }
}