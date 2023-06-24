using System;
using TheArchive.Loader;

namespace TheArchive.Features.Dev
{
    public partial class ModSettings
    {
#if IL2CPP
        public static void RegisterReceiverTypesInIL2CPP()
        {
            LoaderWrapper.ClassInjector.RegisterTypeInIl2CppWithInterfaces<CustomStringReceiver>(true, typeof(iStringInputReceiver));
        }
#endif

        public class CustomStringReceiver
#if MONO
            : iStringInputReceiver
        {
            public CustomStringReceiver(Func<string> getFunc, Action<string> setAction)
            {
                _getValue = getFunc;
                _setValue = setAction;
            }
#else
            : Il2CppSystem.Object
        {
            public CustomStringReceiver(IntPtr ptr) : base(ptr)
            {
            }

            public CustomStringReceiver(Func<string> getFunc, Action<string> setAction) : base(LoaderWrapper.ClassInjector.DerivedConstructorPointer<CustomStringReceiver>())
            {
                LoaderWrapper.ClassInjector.DerivedConstructorBody(this);

                _getValue = getFunc;
                _setValue = setAction;
            }
#endif

            private readonly Func<string> _getValue;
            private readonly Action<string> _setValue;

            string
#if MONO
                iStringInputReceiver.
#endif
                GetStringValue(eCellSettingID setting)
            {
                return _getValue?.Invoke() ?? string.Empty;
            }

            string
#if MONO
                iStringInputReceiver.
#endif
                SetStringValue(eCellSettingID setting, string value)
            {
                _setValue.Invoke(value);
                return value;
            }

        }

    }
}
