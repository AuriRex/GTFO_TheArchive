using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Interfaces;

namespace TheArchive.Core.Localization;

internal abstract class BaseLocalizationService : ILocalizationService
{
    protected readonly IArchiveLogger Logger;
    private readonly Dictionary<ILocalizedTextSetter, (uint, string)> _textSetters = new();
    private readonly HashSet<ILocalizedTextUpdater> _textUpdaters = new();

    public string Identifier { get; }
    
    internal BaseLocalizationService(IArchiveLogger logger, string identifier)
    {
        Logger = logger;
        Identifier = identifier;
    }
    
    public Language CurrentLanguage { get; private set; }

    public void SetCurrentLanguage(Language language)
    {
        CurrentLanguage = language;
        UpdateAllTexts();
    }

    public abstract string GetById(uint id, string fallback = null);

    public virtual string Get<T>(T value)
    {
        return Get(typeof(T), value);
    }

    public abstract string Get(Type type, object value);

    public virtual string Format(uint id, string fallback = null, params object[] args)
    {
        try
        {
            return string.Format(GetById(id, fallback), args);
        }
        catch (FormatException ex)
        {
            var message = $"{nameof(FormatException)} thrown in {nameof(Format)} for id {id}!";
            Logger.Error(message);
            Logger.Exception(ex);
            return message;
        }
    }

    public virtual void AddTextSetter(ILocalizedTextSetter textSetter, uint textId, string fallback = null)
    {
        textSetter.SetText(GetById(textId, fallback));
        _textSetters.Add(textSetter, (textId, fallback));
    }

    public virtual void AddTextUpdater(ILocalizedTextUpdater textUpdater)
    {
        textUpdater.UpdateText();
        _textUpdaters.Add(textUpdater);
    }

    public virtual void RemoveTextSetter(ILocalizedTextSetter textSetter)
    {
        _textSetters.Remove(textSetter);
    }
    
    public virtual void RemoveTextUpdater(ILocalizedTextUpdater textUpdater)
    {
        _textUpdaters.Remove(textUpdater);
    }

    public virtual void UpdateAllTexts()
    {
        foreach ((var service, (var id, var fallback)) in _textSetters)
        {
            service.SetText(GetById(id, fallback));
        }
        foreach (var localizedTextUpdater in _textUpdaters)
        {
            localizedTextUpdater.UpdateText();
        }
    }

    protected string GetSingleEnumText<T>(T value, Dictionary<string, string> enumTexts)
    {
        string enumName = value.ToString()!;
        if (enumTexts.TryGetValue(enumName, out var localizedText) &&
            !string.IsNullOrWhiteSpace(localizedText))
        {
            return localizedText;
        }
        return enumName;
    }

    protected string GetFlagsEnumText<T>(Type type, T value, Dictionary<string, string> enumTexts)
    {
        var valueAsLong = Convert.ToInt64(value);

        if (valueAsLong == 0)
        {
            return GetSingleEnumText(value, enumTexts);
        }

        List<string> result = new();

        var enumValues = Enum.GetValues(type)
            .Cast<object>()
            .Where(v => Convert.ToInt64(v) != 0)
            .OrderByDescending(v => Convert.ToInt64(v))
            .ToArray();

        var remainingValue = valueAsLong;

        foreach (var enumValue in enumValues)
        {
            var enumValueAsLong = Convert.ToInt64(enumValue);

            if ((remainingValue & enumValueAsLong) == enumValueAsLong)
            {
                string enumName = enumValue.ToString()!;
                if (enumTexts.TryGetValue(enumName, out var localizedText) &&
                    !string.IsNullOrWhiteSpace(localizedText))
                {
                    result.Add(localizedText);
                    remainingValue &= ~enumValueAsLong;
                }
                else
                {
                    return value.ToString()!;
                }
            }
        }

        if (remainingValue != 0)
        {
            return value.ToString()!;
        }

        return result.Count > 0 ? string.Join(", ", result) : value.ToString()!;
    }
}