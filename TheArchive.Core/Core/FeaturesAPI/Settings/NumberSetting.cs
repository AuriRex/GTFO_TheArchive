using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class NumberSetting : FeatureSetting
    {
        public NumberFormat Format { get; private set; }
        public NumberSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
            switch(Type.Name)
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

        public override void SetValue(object value)
        {
            base.SetValue(ConvertNumber(value.ToString()));
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
}
