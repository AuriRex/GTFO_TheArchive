using System;
using TheArchive.Loader;

namespace TheArchive.Features.Dev;

public partial class ModSettings
{
#if IL2CPP
    public static void RegisterReceiverTypesInIL2CPP()
    {
        LoaderWrapper.ClassInjector.RegisterTypeInIl2CppWithInterfaces<CustomStringReceiver>(true, typeof(iStringInputReceiver));
        LoaderWrapper.ClassInjector.RegisterTypeInIl2CppWithInterfaces<CustomIntReceiver>(true, typeof(iIntInputReceiver));
        LoaderWrapper.ClassInjector.RegisterTypeInIl2CppWithInterfaces<CustomFloatReceiver>(true, typeof(iFloatInputReceiver));
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

    public class CustomIntReceiver
#if MONO
            : iIntInputReceiver
        {
            public CustomIntReceiver(Func<int> getFunc, Action<int> setAction)
            {
                _getValue = getFunc;
                _setValue = setAction;
            }
#else
        : Il2CppSystem.Object
    {
        public CustomIntReceiver(IntPtr ptr) : base(ptr)
        {
        }

        public CustomIntReceiver(Func<int> getFunc, Action<int> setAction) : base(LoaderWrapper.ClassInjector.DerivedConstructorPointer<CustomIntReceiver>())
        {
            LoaderWrapper.ClassInjector.DerivedConstructorBody(this);

            _getValue = getFunc;
            _setValue = setAction;
        }
#endif

        private readonly Func<int> _getValue;
        private readonly Action<int> _setValue;

        int
#if MONO
                iIntInputReceiver.
#endif
            GetIntValue(eCellSettingID setting)
        {
            return _getValue?.Invoke() ?? 0;
        }

        int
#if MONO
                iIntInputReceiver.
#endif
            SetIntValue(eCellSettingID setting, int value)
        {
            _setValue.Invoke(value);
            return value;
        }

    }

    public class CustomFloatReceiver
#if MONO
            : iFloatInputReceiver
        {
            public CustomFloatReceiver(Func<float> getFunc, Action<float> setAction)
            {
                _getValue = getFunc;
                _setValue = setAction;
            }
#else
        : Il2CppSystem.Object
    {
        public CustomFloatReceiver(IntPtr ptr) : base(ptr)
        {
        }

        public CustomFloatReceiver(Func<float> getFunc, Action<float> setAction) : base(LoaderWrapper.ClassInjector.DerivedConstructorPointer<CustomFloatReceiver>())
        {
            LoaderWrapper.ClassInjector.DerivedConstructorBody(this);

            _getValue = getFunc;
            _setValue = setAction;
        }
#endif

        private readonly Func<float> _getValue;
        private readonly Action<float> _setValue;

        float
#if MONO
                iFloatInputReceiver.
#endif
            GetFloatValue(eCellSettingID setting)
        {
            return _getValue?.Invoke() ?? 0f;
        }

        float
#if MONO
                iFloatInputReceiver.
#endif
            SetFloatValue(eCellSettingID setting, float value)
        {
            _setValue.Invoke(value);
            return value;
        }

    }
}