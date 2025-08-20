using System;
using JetBrains.Annotations;
using TheArchive.Loader;

namespace TheArchive.Features.Dev;

public partial class ModSettings
{
#if IL2CPP
    private static void RegisterReceiverTypesInIL2CPP()
    {
        LoaderWrapper.ClassInjector.RegisterTypeInIl2CppWithInterfaces<CustomStringReceiver>(true, typeof(iStringInputReceiver));
        LoaderWrapper.ClassInjector.RegisterTypeInIl2CppWithInterfaces<CustomIntReceiver>(true, typeof(iIntInputReceiver));
        LoaderWrapper.ClassInjector.RegisterTypeInIl2CppWithInterfaces<CustomFloatReceiver>(true, typeof(iFloatInputReceiver));
    }
#endif

    /// <summary>
    /// Implements the games <see cref="iStringInputReceiver"/> interface.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
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
        /// <summary>
        /// Il2Cpp object constructor.
        /// </summary>
        /// <param name="ptr">Instance pointer.</param>
        public CustomStringReceiver(IntPtr ptr) : base(ptr)
        {
        }

        /// <summary>
        /// Creates a custom string receiver.
        /// </summary>
        /// <param name="getFunc">Get func.</param>
        /// <param name="setAction">Set action.</param>
        public CustomStringReceiver(Func<string> getFunc, Action<string> setAction) : base(LoaderWrapper.ClassInjector.DerivedConstructorPointer<CustomStringReceiver>())
        {
            LoaderWrapper.ClassInjector.DerivedConstructorBody(this);

            _getValue = getFunc;
            _setValue = setAction;
        }
#endif

        private readonly Func<string> _getValue;
        private readonly Action<string> _setValue;

        private string
#if MONO
                iStringInputReceiver.
#endif
            GetStringValue(eCellSettingID setting)
        {
            return _getValue?.Invoke() ?? string.Empty;
        }

        private string
#if MONO
                iStringInputReceiver.
#endif
            SetStringValue(eCellSettingID setting, string value)
        {
            _setValue.Invoke(value);
            return value;
        }
    }

    /// <summary>
    /// Implements the games <see cref="iIntInputReceiver"/> interface.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
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
        /// <summary>
        /// Il2Cpp object constructor.
        /// </summary>
        /// <param name="ptr">Instance pointer.</param>
        public CustomIntReceiver(IntPtr ptr) : base(ptr)
        {
        }

        /// <summary>
        /// Creates a custom int receiver.
        /// </summary>
        /// <param name="getFunc">Get func.</param>
        /// <param name="setAction">Set action.</param>
        public CustomIntReceiver(Func<int> getFunc, Action<int> setAction) : base(LoaderWrapper.ClassInjector.DerivedConstructorPointer<CustomIntReceiver>())
        {
            LoaderWrapper.ClassInjector.DerivedConstructorBody(this);

            _getValue = getFunc;
            _setValue = setAction;
        }
#endif

        private readonly Func<int> _getValue;
        private readonly Action<int> _setValue;

        private int
#if MONO
                iIntInputReceiver.
#endif
            GetIntValue(eCellSettingID setting)
        {
            return _getValue?.Invoke() ?? 0;
        }

        private int
#if MONO
                iIntInputReceiver.
#endif
            SetIntValue(eCellSettingID setting, int value)
        {
            _setValue.Invoke(value);
            return value;
        }
    }

    /// <summary>
    /// Implements the games <see cref="iFloatInputReceiver"/> interface.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
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
        /// <summary>
        /// Il2Cpp object constructor.
        /// </summary>
        /// <param name="ptr">Instance pointer.</param>
        public CustomFloatReceiver(IntPtr ptr) : base(ptr)
        {
        }

        /// <summary>
        /// Creates a custom float receiver.
        /// </summary>
        /// <param name="getFunc">Get func.</param>
        /// <param name="setAction">Set action.</param>
        public CustomFloatReceiver(Func<float> getFunc, Action<float> setAction) : base(LoaderWrapper.ClassInjector.DerivedConstructorPointer<CustomFloatReceiver>())
        {
            LoaderWrapper.ClassInjector.DerivedConstructorBody(this);

            _getValue = getFunc;
            _setValue = setAction;
        }
#endif

        private readonly Func<float> _getValue;
        private readonly Action<float> _setValue;

        private float
#if MONO
                iFloatInputReceiver.
#endif
            GetFloatValue(eCellSettingID setting)
        {
            return _getValue?.Invoke() ?? 0f;
        }

        private float
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