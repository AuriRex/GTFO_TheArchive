using System;
using System.Collections.Generic;
using TheArchive.Interfaces;

namespace TheArchive.Core.Localization;

internal abstract class BaseLocalizationService : ILocalizationService
{
    protected readonly IArchiveLogger Logger;
    private readonly Dictionary<ILocalizedTextSetter, uint> _textSetters = new();
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

    public abstract string Get(uint id);


    public virtual string Get<T>(T value) where T : Enum
    {
        return Get(typeof(T), value);
    }

    public abstract string Get(Type type, object value);

    public virtual string Format(uint id, params object[] args)
    {
        try
        {
            return string.Format(Get(id), args);
        }
        catch (FormatException ex)
        {
            var message = $"{nameof(FormatException)} thrown in {nameof(Format)} for id {id}!";
            Logger.Error(message);
            Logger.Exception(ex);
            return message;
        }
    }

    public virtual void AddTextSetter(ILocalizedTextSetter textSetter, uint textId)
    {
        textSetter.SetText(Get(textId));
        _textSetters.Add(textSetter, textId);
    }

    public virtual void SetTextSetter(ILocalizedTextSetter textSetter, uint textId)
    {
        textSetter.SetText(Get(textId));
        _textSetters[textSetter] = textId;
    }

    public virtual void AddTextUpdater(ILocalizedTextUpdater textUpdater)
    {
        textUpdater.UpdateText();
        _textUpdaters.Add(textUpdater);
    }

    public virtual void UpdateAllTexts()
    {
        foreach (var keyValuePair in _textSetters)
        {
            keyValuePair.Key.SetText(Get(keyValuePair.Value));
        }
        foreach (var localizedTextUpdater in _textUpdaters)
        {
            localizedTextUpdater.UpdateText();
        }
    }
}