using CellMenu;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TMPro;
using UnityEngine;
using GameData;
using System.Runtime.CompilerServices;
using TheArchive.Loader;
using SNetwork;
using LevelGeneration;
using static TheArchive.Core.FeaturesAPI.Feature;
#if Unhollower
using UnhollowerBaseLib;
#endif
#if Il2CppInterop
using Il2CppInterop.Runtime.InteropTypes;
#endif
#if IL2CPP
using IL2ColGen = Il2CppSystem.Collections.Generic;
#endif

namespace TheArchive.Utilities;

/// <summary>
/// Random utility methods related to Il2Cpp/Mono things.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class SharedUtils
{

#if MONO
    public static FieldAccessor<CM_Item, Color> A_CM_Item_m_spriteColorOrg = FieldAccessor<CM_Item, Color>.GetAccessor("m_spriteColorOrg");
    public static FieldAccessor<CM_Item, Color> A_CM_Item_m_spriteColorOut = FieldAccessor<CM_Item, Color>.GetAccessor("m_spriteColorOut");
    public static FieldAccessor<CM_Item, Color> A_CM_Item_m_spriteColorOver = FieldAccessor<CM_Item, Color>.GetAccessor("m_spriteColorOver");

    public static FieldAccessor<CM_Item, Color[]> A_CM_Item_m_textColorOrg = FieldAccessor<CM_Item, Color[]>.GetAccessor("m_textColorOrg");
    public static FieldAccessor<CM_Item, Color[]> A_CM_Item_m_textColorOut = FieldAccessor<CM_Item, Color[]>.GetAccessor("m_textColorOut");
    public static FieldAccessor<CM_Item, Color[]> A_CM_Item_m_textColorOver = FieldAccessor<CM_Item, Color[]>.GetAccessor("m_textColorOver");
#endif

#if IL2CPP
    /// <summary>
    /// Turns an Il2Cpp list into a managed one.
    /// </summary>
    /// <param name="il2List">The Il2Cpp list to convert.</param>
    /// <typeparam name="T">The list type.</typeparam>
    /// <returns>A managed representation of the list.</returns>
    public static List<T> ToSystemList<T>(this IL2ColGen.List<T> il2List)
    {
        var list = new List<T>();

        foreach (var item in il2List)
        {
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// Turns a managed list into an Il2Cpp one.
    /// </summary>
    /// <param name="list">The managed list to convert.</param>
    /// <typeparam name="T">The list type.</typeparam>
    /// <returns>An Il2Cpp representation of the list.</returns>
    /// <remarks>
    /// Mostly a legacy method - check legacy branch.
    /// </remarks>
    public static IL2ColGen.List<T> ToIL2CPPListIfNecessary<T>(this List<T> list)
    {
        var il2List = new IL2ColGen.List<T>();
        foreach (var item in list)
        {
            il2List.Add(item);
        }
        return il2List;
    }

    /// <summary>
    /// Creates a new Il2Cpp list of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The list type.</typeparam>
    /// <returns>A new Il2Cpp list.</returns>
    /// <remarks>
    /// Mostly a legacy method - check legacy branch.
    /// </remarks>
    public static IL2ColGen.List<T> NewListForGame<T>()
    {
        return new IL2ColGen.List<T>();
    }
#else
    public static List<T> ToSystemList<T>(this List<T> list) => list;
    public static List<T> ToIL2CPPListIfNecessary<T>(this List<T> list) => list;
    public static List<T> NewListForGame<T>()
    {
        return new List<T>();
    }
#endif

    /// <summary>
    /// Change the color of the <c>CM_TimedButton</c> texts and sprite renderers.
    /// </summary>
    /// <param name="button">The button to recolor.</param>
    /// <param name="col">The new color.</param>
    public static void ChangeColorTimedExpeditionButton(CM_TimedButton button, Color col) => ChangeColorOnAllChildren(button.transform, col, new[] { "ProgressFill" });

    /// <summary>
    /// Change the colors of a <c>CM_Item</c>.
    /// </summary>
    /// <param name="item">The <c>CM_Item</c> to recolor.</param>
    /// <param name="idleColor">The idle color.</param>
    /// <param name="hoverColor">The color used while the item is hovered over.</param>
    public static void ChangeColorCMItem(CM_Item item, Color idleColor, Color? hoverColor = null)
    {
        ChangeColorOnSelfAndAllChildren(item.transform, idleColor);
        var textColorsOut = new List<Color>();
        var textColorsOver = new List<Color>();
        var colorOut = idleColor.WithAlpha(.5f);
        var colorOver = hoverColor ?? idleColor.WithAlpha(1f);
#if IL2CPP
        item.m_spriteColorOrg = colorOut;
        item.m_spriteColorOut = colorOut;
        item.m_spriteColorOver = colorOver;
        if (item.m_textColorOrg != null)
            foreach (var _ in item.m_textColorOrg)
            {
                textColorsOut.Add(colorOut);
                textColorsOver.Add(colorOver);
            }
        item.m_textColorOrg = textColorsOut.ToArray();
        item.m_textColorOut = textColorsOut.ToArray();
        item.m_textColorOver = textColorsOver.ToArray();
#else
        A_CM_Item_m_spriteColorOrg.Set(item, colorOut);
        A_CM_Item_m_spriteColorOut.Set(item, colorOut);
        A_CM_Item_m_spriteColorOver.Set(item, colorOver);
        var m_textColorOrg = A_CM_Item_m_textColorOrg.Get(item);
        if (m_textColorOrg != null)
            foreach (var textCol in m_textColorOrg)
            {
                textColorsOut.Add(colorOut);
                textColorsOver.Add(colorOver);
            }
        A_CM_Item_m_textColorOrg.Set(item, textColorsOut.ToArray());
        A_CM_Item_m_textColorOut.Set(item, textColorsOut.ToArray());
        A_CM_Item_m_textColorOver.Set(item, textColorsOver.ToArray());
#endif
    }

    /// <summary>
    /// Change the colors of all <c>TextMeshPro</c> and <c>SpriteRenderer</c>s on self and on all children.
    /// </summary>
    /// <param name="trans">The transform to recolor.</param>
    /// <param name="col">The new color.</param>
    /// <param name="excludeNames">A list of transform names to ignore.</param>
    /// <param name="mode">The check mode of how to apply the <paramref name="excludeNames"/> list.</param>
    /// <param name="extraModificationForEachChild">Custom code ran on each child.</param>
    public static void ChangeColorOnSelfAndAllChildren(Transform trans, Color col, IList<string> excludeNames = null, IgnoreMode mode = IgnoreMode.StartsWith, Action<Transform> extraModificationForEachChild = null)
    {
        ChangeColor(trans, col, excludeNames, mode, extraModificationForEachChild);
        ChangeColorOnAllChildren(trans, col, excludeNames, mode, extraModificationForEachChild);
    }

    /// <inheritdoc cref="ChangeColorOnSelfAndAllChildren"/>
    /// <summary>
    /// Change the colors of all <c>TextMeshPro</c> and <c>SpriteRenderer</c>s on all children.
    /// </summary>
    public static void ChangeColorOnAllChildren(Transform trans, Color col, IList<string> excludeNames = null, IgnoreMode mode = IgnoreMode.StartsWith, Action<Transform> extraModificationForEachChild = null)
    {
        if (trans == null) return;
        trans.ForEachChildDo(child => {
            ChangeColor(child?.transform, col, excludeNames, mode, extraModificationForEachChild);
        });
    }

    /// <inheritdoc cref="ChangeColorOnSelfAndAllChildren"/>
    /// <summary>
    /// Change the colors of all <c>TextMeshPro</c> and <c>SpriteRenderer</c>s on this transforms' gameobject.
    /// </summary>
    public static void ChangeColor(Transform trans, Color col, IList<string> excludeNames = null, IgnoreMode mode = IgnoreMode.StartsWith, Action<Transform> extraModificationForEachChild = null)
    {
        if (trans == null) return;
        if (excludeNames != null)
        {
            switch (mode)
            {
                case IgnoreMode.Match:
                    if (excludeNames.Contains(trans.name))
                        return;
                    break;
                case IgnoreMode.StartsWith:
                    if (excludeNames.Any(s => trans.name.StartsWith(s)))
                        return;
                    break;
                case IgnoreMode.EndsWith:
                    if (excludeNames.Any(s => trans.name.EndsWith(s)))
                        return;
                    break;
            }
        }
        var spriteRenderer = trans.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = col;
        }
        var tmp = trans.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.color = col;
        }
        extraModificationForEachChild?.Invoke(trans);
    }

    /// <summary>
    /// Clear all subscribers on an event.
    /// </summary>
    /// <param name="eventFieldName">The name of the event member.</param>
    /// <param name="instance">The object instance containing the event action or null if static.</param>
    /// <typeparam name="T">The type containing the event member.</typeparam>
    public static void RemoveAllEventHandlers<T>(string eventFieldName, object instance = null)
    {
#if IL2CPP
        typeof(T).GetProperty(eventFieldName, Utils.AnyBindingFlagss).SetValue(instance, null);
#else
        MonoUtils.RemoveAllEventHandlers<T>(eventFieldName, instance);
#endif
    }

    /// <summary>
    /// Add events to a <c>CM_Item</c>.
    /// </summary>
    /// <param name="item">The <c>CM_Item</c> to subscribe to.</param>
    /// <param name="onButtonPress">On button press action to add.</param>
    /// <param name="onButtonHover">On button hover action to add.</param>
    /// <returns>The same <c>CM_Item</c>.</returns>
    /// <exception cref="ArgumentNullException">Item can't be null.</exception>
    public static CM_Item AddCMItemEvents(this CM_Item item, Action<int> onButtonPress, Action<int, bool> onButtonHover = null)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        
        if (onButtonPress != null)
            item.OnBtnPressCallback += onButtonPress;
        if (onButtonHover != null)
            item.OnBtnHoverChanged += onButtonHover;

        return item;
    }

    /// <summary>
    /// Set events of a <c>CM_Item</c>.
    /// </summary>
    /// <inheritdoc cref="AddCMItemEvents"/>
    public static CM_Item SetCMItemEvents(this CM_Item item, Action<int> onButtonPress, Action<int, bool> onButtonHover = null)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        if(item.m_onBtnPress == null)
            item.m_onBtnPress = new UnityEngine.Events.UnityEvent();

#if IL2CPP
        if(onButtonPress != null)
            item.OnBtnPressCallback = onButtonPress;
        if(onButtonHover != null)
            item.OnBtnHoverChanged = onButtonHover;
#else
        MonoUtils.RemoveAllEventHandlers<CM_Item>(nameof(CM_Item.OnBtnHoverChanged), item);
        MonoUtils.RemoveAllEventHandlers<CM_Item>(nameof(CM_Item.OnBtnPressCallback), item);

        if(onButtonPress!= null)
            item.OnBtnPressCallback += onButtonPress;
        if(onButtonHover != null)
            item.OnBtnHoverChanged += onButtonHover;
#endif

        return item;
    }

    /// <summary>
    /// Remove all events on a <c>CM_Item</c>.
    /// </summary>
    /// <param name="item">The <c>CM_Item</c> to clear all events on.</param>
    /// <param name="keepHover">Should hover events be kept?</param>
    /// <returns>The same <c>CM_Item</c>.</returns>
    public static CM_Item RemoveCMItemEvents(this CM_Item item, bool keepHover = false)
    {
        RemoveAllEventHandlers<CM_Item>(nameof(CM_Item.OnBtnPressCallback), item);
        if(!keepHover)
            RemoveAllEventHandlers<CM_Item>(nameof(CM_Item.OnBtnHoverChanged), item);

        return item;
    }

    /// <summary>
    /// Sets a <c>CM_TimedButton</c> hold duration.
    /// </summary>
    /// <param name="button">The button to set the duration for.</param>
    /// <param name="duration">The new hold duration.</param>
    public static void SetHoldDuration(this CM_TimedButton button, float duration)
    {
#if IL2CPP
        button.m_holdButtonDuration = duration;
#else
        FieldAccessor<CM_TimedButton, float>.GetAccessor("m_holdButtonDuration").Set(button, duration);
#endif
    }

    /// <summary>
    /// Creates the same <see cref="Color"/> but with a different alpha value
    /// </summary>
    /// <param name="col">Original <see cref="Color"/></param>
    /// <param name="alpha">New alpha value</param>
    /// <returns>New <see cref="Color"/></returns>
    public static Color WithAlpha(this Color col, float alpha)
    {
        return new Color(col.r, col.g, col.b, alpha);
    }

    /// <summary>
    /// Run <paramref name="func"/> on every first child of <paramref name="trans"/>
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="func"></param>
    public static void ForEachFirstChildDo(this Transform trans, Action<Transform> func) => trans.ForEachChildDo(func, recursive: false);

    /// <summary>
    /// Run <paramref name="func"/> on every child of <paramref name="trans"/>
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="func"></param>
    /// <param name="recursive">If the children of every child should be included</param>
    public static void ForEachChildDo(this Transform trans, Action<Transform> func, bool recursive = true)
    {
        if (trans == null) throw new ArgumentNullException(nameof(trans));
        for (var i = 0; i < trans.childCount; i++)
        {
            var child = trans.GetChild(i);
            func?.Invoke(child);
            if (recursive)
            {
                ForEachChildDo(child, func);
            }
        }
    }

    /// <summary>
    /// Get all direct children of this transform.
    /// </summary>
    /// <param name="trans">The transform to get the children from.</param>
    /// <returns><c>IEnumerable&lt;Transform&gt;</c> of all direct children.</returns>
    public static IEnumerable<Transform> DirectChildren(this Transform trans)
    {
        for (var i = 0; i < trans.childCount; i++)
        {
            yield return trans.GetChild(i);
        }
    }

    /// <summary>
    /// Get a direct child with an exact name match.
    /// </summary>
    /// <param name="trans">The transform containing the child.</param>
    /// <param name="name">The name of the child to match.</param>
    /// <returns>The found child or null.</returns>
    public static Transform GetChildWithExactName(this Transform trans, string name)
    {
        for (var i = 0; i < trans.childCount; i++)
        {
            var child = trans.GetChild(i);
            if (child.name == name) return child;
        }
        return null;
    }

    /// <summary>
    /// Destroy a gameobject but null-checked.
    /// </summary>
    /// <param name="go">The gameobject to destroy.</param>
    public static void SafeDestroyGO(this GameObject go) => go.SafeDestroy();

    /// <summary>
    /// Destroy a gameobject but null-checked.
    /// </summary>
    /// <param name="comp">Destroys the gameobject this component is attached to.</param>
    public static void SafeDestroyGO(this Component comp) => comp?.gameObject.SafeDestroy();

    /// <inheritdoc cref="SafeDestroyGO(GameObject)"/>
    public static void SafeDestroy(this GameObject go)
    {
        if (go == null)
            return;
        UnityEngine.Object.Destroy(go);
    }

    /// <summary>
    /// Destroy a component but null-checked.
    /// </summary>
    /// <param name="comp">The component to destroy.</param>
    public static void SafeDestroy(this Component comp)
    {
        if (comp == null)
            return;
        UnityEngine.Object.Destroy(comp);
    }

    /// <summary>
    /// Ignore mode
    /// </summary>
    public enum IgnoreMode
    {
        /// <summary> Has to match exactly. </summary>
        Match,
        /// <summary> Has to start with. </summary>
        StartsWith,
        /// <summary> Has to end with. </summary>
        EndsWith
    }

    private static readonly IValueAccessor<CM_PageLoadout, CM_PlayerLobbyBar[]> _A_CM_PageLoadout_m_playerLobbyBars = AccessorBase.GetValueAccessor<CM_PageLoadout, CM_PlayerLobbyBar[]>("m_playerLobbyBars");
    private static readonly IValueAccessor<CM_PlayerLobbyBar, int> _A_CM_PlayerLobbyBar_m_playerSlotIndex = AccessorBase.GetValueAccessor<CM_PlayerLobbyBar, int>("m_playerSlotIndex");
    private static readonly IValueAccessor<CM_PlayerLobbyBar, int> _A_CM_PlayerLobbyBar_m_playerIndex = AccessorBase.GetValueAccessor<CM_PlayerLobbyBar, int>("m_playerIndex"); // Used in older versions
    private static readonly IValueAccessor<CM_PlayerLobbyBar, SNet_Player> _A_CM_PlayerLobbyBar_m_player = AccessorBase.GetValueAccessor<CM_PlayerLobbyBar, SNet_Player>("m_player");

    /// <summary>
    /// Get the index of a given <c>CM_PlayerLobbyBar</c>.
    /// </summary>
    /// <param name="plb">The player lobby bar to get the index of.</param>
    /// <param name="index">The index of the player lobby bar.</param>
    /// <returns></returns>
    public static bool TryGetPlayerLobbyBarIndex(CM_PlayerLobbyBar plb, out int index)
    {
        index = GetPlayerLobbyBarIndex(plb);
        return index >= 0;
    }

    /// <summary>
    /// Get the index of a given <c>CM_PlayerLobbyBar</c>.
    /// </summary>
    /// <param name="plb">The player lobby bar to get the index of.</param>
    /// <returns>The index of the player lobby bar or -1.</returns>
    public static int GetPlayerLobbyBarIndex(CM_PlayerLobbyBar plb)
    {
        return _A_CM_PlayerLobbyBar_m_playerSlotIndex?.Get(plb) ?? _A_CM_PlayerLobbyBar_m_playerIndex?.Get(plb) ?? -1;
    }

    /// <summary>
    /// Get the <c>SNet_Player</c> from a given <c>CM_PlayerLobbyBar</c> index.
    /// </summary>
    /// <param name="index">The index of the <c>CM_PlayerLobbyBar</c>.</param>
    /// <param name="player">The found player.</param>
    /// <returns><c>True</c> if found.</returns>
    public static bool TryGetPlayerByPlayerLobbyBarIndex(int index, out SNet_Player player)
    {
#if IL2CPP
        var lobbyBars = CM_PageLoadout.Current.m_playerLobbyBars.ToArray();
#else
        var lobbyBars = _A_CM_PageLoadout_m_playerLobbyBars.Get(CM_PageLoadout.Current);
#endif

        var resultPlayerLobbyBar = lobbyBars.FirstOrDefault(plb => GetPlayerLobbyBarIndex(plb) == index);

        if (resultPlayerLobbyBar == null)
        {
            player = null;
            return false;
        }

        player = _A_CM_PlayerLobbyBar_m_player.Get(resultPlayerLobbyBar);
        return player != null;
    }

    /// <summary>
    /// Get the first <c>SNet_Player</c> matching a given character index.
    /// </summary>
    /// <param name="id">The character index.</param>
    /// <param name="player">The found player.</param>
    /// <returns><c>True</c> if found.</returns>
    public static bool TryGetPlayerByCharacterIndex(int id, out SNet_Player player)
    {
        try
        {
            player = SNet.Lobby.Players
#if IL2CPP
                .ToSystemList()
#endif
                .FirstOrDefault(ply => ply.CharacterSlot.index == id);

            return player != null;
        }
        catch (Exception)
        {
            player = null;
            ArchiveLogger.Debug($"This shouldn't happen :skull: ({nameof(TryGetPlayerByCharacterIndex)})");
        }
        return false;
    }

    /// <summary>
    /// Get the bounds of all active renderers of the given gameobject.
    /// </summary>
    /// <param name="go">The gameobject to check.</param>
    /// <returns>The bounds of all active renderers.</returns>
    /// <seealso href="https://gamedev.stackexchange.com/a/183962"/>
    public static Bounds GetMaxBounds(this GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    /// <summary>
    /// Check if a list contains a given item Il2Cpp type safe.<br/>
    /// If the list type is an Il2Cpp type it checks the Il2Cpp object pointer instead.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="item">The item to check.</param>
    /// <typeparam name="T">The type of the list item.</typeparam>
    /// <returns><c>True</c> if the item is contained in the list.</returns>
    public static bool SafeContains<T>(this IList<T> list, T item) where T : class, new()
    {
        if (LoaderWrapper.IsIL2CPPType(typeof(T)))
        {
#if IL2CPP
            foreach(var listItem in list)
            {
                var il2Object = listItem as Il2CppSystem.Object;
                if (il2Object.Pointer == (item as Il2CppSystem.Object).Pointer)
                    return true;
            }
            return false;
#else
            throw new InvalidOperationException("This should never be called!");
#endif
        }

        return list.Contains(item);
    }

    /// <summary>
    /// Safely check if a <c>SNet_Player</c> is a bot.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns></returns>
    /// <remarks>
    /// Legacy method - was used to safely check across different game versions.<br/>
    /// Not needed anymore.
    /// </remarks>
    public static bool SafeIsBot(this SNet_Player player)
    {
        if (player == null)
            return false;
        if (Is.R6OrLater)
            return IsBotR6(player);
        return false;
    }

    /// <summary>
    /// Check if a <c>SNet_Player</c> is your steam friend.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns><c>True</c> if the given player is your steam friend.</returns>
    public static bool IsFriend(this SNet_Player player)
    {
        if (player == null)
            return false;
        return SNet.Friends.TryGetFriend(player.Lookup, out _);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsBotR6(SNet_Player player)
    {
#if IL2CPP
        return player.IsBot;
#else
        return false;
#endif
    }

    private static readonly MethodAccessor<CellSoundPlayer> A_CellSoundPlayer_Post_sub_R5 = MethodAccessor<CellSoundPlayer>.GetAccessor("Post", new[] { typeof(uint) }, ignoreErrors: true);

    /// <summary>
    /// Post a sound event.
    /// </summary>
    /// <param name="player">The sound player to use.</param>
    /// <param name="eventId">The sound event to play.</param>
    /// <param name="isGlobal">Is global?</param>
    /// <remarks>
    /// Legacy method - was used to safely post sounds across different game versions.<br/>
    /// Not needed anymore.
    /// </remarks>
    public static void SafePost(this CellSoundPlayer player, uint eventId, bool isGlobal = true)
    {
        if (Is.R6OrLater)
        {
            SafePostR6Plus(player, eventId, isGlobal);
            return;
        }
        A_CellSoundPlayer_Post_sub_R5.Invoke(player, eventId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SafePostR6Plus(CellSoundPlayer player, uint eventId, bool isGlobal = true)
    {
#if IL2CPP
        player.Post(eventId, isGlobal);
#endif
    }

#if IL2CPP
    private static readonly PropertyAccessor<GameSetupDataBlock, uint> _rundownIdToLoad = PropertyAccessor<GameSetupDataBlock, uint>.GetAccessor("RundownIdToLoad");
        
    private static GameSetupDataBlock _setupBlock;
#endif

    /// <summary>
    /// Get the currently active rundown datablock.
    /// </summary>
    /// <param name="block">The active rundown datablock.</param>
    /// <returns><c>True</c> if a valid rundown datablock is active.</returns>
    public static bool TryGetRundownDataBlock(out RundownDataBlock block)
    {
        uint blockToLoad;
#if IL2CPP
        if (ArchiveMod.IsOnALTBuild)
        {
            if(RundownManager.ActiveExpedition != null)
            {
                var rundownKeyFromExpedition = RundownManager.GetActiveExpeditionData()?.rundownKey?.data;
                if (rundownKeyFromExpedition != null && !rundownKeyFromExpedition.StartsWith("pString") && rundownKeyFromExpedition != ArchiveMod.CurrentlySelectedRundownKey)
                {
                    ArchiveMod.CurrentlySelectedRundownKey = rundownKeyFromExpedition;
                }
            }

            blockToLoad = ArchiveMod.CurrentlySelectedRundownPersistentID;
        }
        else
        {
            _setupBlock ??= GameSetupDataBlock.GetBlock(1);
            blockToLoad = _rundownIdToLoad.Get(_setupBlock);
        }
#else
        blockToLoad = Globals.Global.RundownIdToLoad;
#endif
        if (blockToLoad != 0)
        {
            block = RundownDataBlock.GetBlock(blockToLoad);
            return true;
        }

        block = null;
        return false;
    }

    /// <summary>
    /// Get the title of the currently active rundown datablock.
    /// </summary>
    /// <returns>The rundowns title or <c>"Unknown"</c></returns>
    public static string GetDataBlockRundownTitle()
    {
        string text = null;
        if (TryGetRundownDataBlock(out var block))
        {
            text = Utils.StripTMPTagsRegex(block.StorytellingData.Title);
        }

        if(!ArchiveMod.IsPlayingModded && text != null)
        {
            try
            {
                var split = text.Split("TITLE: ");

                var title = split[1];
                
                var part1 = split[0];
                var rundown = part1.Split('#')[1].Split('.')[0];
                
                text = $"R{rundown} - {title}";
            }
            catch
            {
                // ignored
            }
            //text = text.Replace("TITLE:", "-");
        }

        if(ArchiveMod.IsOnALTBuild && text == null)
            return "Selecting Rundown";

        return text ?? "Unknown";
    }

    private static readonly eFocusState eFocusState_ComputerTerminal = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.ComputerTerminal));
    private static readonly eFocusState eFocusState_Map = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.Map));
    private static readonly eFocusState eFocusState_Dead = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.Dead));
    private static readonly eFocusState eFocusState_InElevator = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.InElevator));
    private static readonly eFocusState eFocusState_Hacking = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.Hacking));

    /// <summary> If the local player is interacting with a terminal. </summary>
    public static bool LocalPlayerIsInTerminal => FocusStateManager.CurrentState == eFocusState_ComputerTerminal;
    
    /// <summary> If the local player is looking at the map. </summary>
    public static bool LocalPlayerIsInMap => FocusStateManager.CurrentState == eFocusState_Map;
    /// <summary> If the local player is currently dead. </summary>
    public static bool LocalPlayerIsDead => FocusStateManager.CurrentState == eFocusState_Dead;
    /// <summary> If the local player is currently in the elevator. </summary>
    public static bool LocalPlayerIsInElevator => FocusStateManager.CurrentState == eFocusState_InElevator;
    /// <summary> If the local player is currently in the hacking minigame. </summary>
    public static bool LocalPlayerIsHacking => FocusStateManager.CurrentState == eFocusState_Hacking;

    private static readonly eGameStateName eGameStateName_InLevel = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.InLevel));

#if IL2CPP
    private static IValueAccessor<LG_Floor, IL2ColGen.List<LG_Layer>> _A_LG_Floor_m_layers;
#endif
#if MONO
    private static IValueAccessor<LG_Floor, List<LG_Zone>> _A_LG_Floor_m_zones;
#endif

    /// <summary>
    /// Returns all zones if in a level (including from every dimension!)
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<LG_Zone> GetAllZones()
    {
        if (GetGameState() != eGameStateName_InLevel)
            yield break;

#if IL2CPP
        if (Is.R6OrLater)
        {
            foreach (var zone_r6 in GetAllZonesR6Plus())
                yield return zone_r6;
            yield break;
        }

        _A_LG_Floor_m_layers ??= AccessorBase.GetValueAccessor<LG_Floor, IL2ColGen.List<LG_Layer>>("m_layers");

        foreach (var layer in _A_LG_Floor_m_layers.Get(Builder.CurrentFloor))
        {
            foreach (var zone in layer.m_zones)
                yield return zone;
        }
#endif
#if MONO
        if(Is.R3OrLater)
        {
            foreach (var zone in GetAllZonesR3Plus())
                yield return zone;
            yield break;
        }
        else
        {
            //R2, R1
            _A_LG_Floor_m_zones ??= AccessorBase.GetValueAccessor<LG_Floor, List<LG_Zone>>("m_zones");

            foreach(var zone in _A_LG_Floor_m_zones.Get(Builder.CurrentFloor))
                yield return zone;
        }
#endif
    }

#if MONO
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<LG_Zone> GetAllZonesR3Plus()
    {
        foreach (var layer in Builder.CurrentFloor.m_layers)
        {
            foreach (var zone in layer.m_zones)
                yield return zone;
        }
    }
#endif

#if IL2CPP
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<LG_Zone> GetAllZonesR6Plus()
    {
        foreach (var dimension in Builder.CurrentFloor.m_dimensions)
        {
            foreach (var layer in dimension.Layers)
            {
                foreach (var zone in layer.m_zones)
                    yield return zone;
            }
        }
    }
#endif

    /// <summary>
    /// Get the currently active gamestate.
    /// </summary>
    /// <returns>The current gamestate.</returns>
    public static eGameStateName GetGameState() => (eGameStateName) ArchiveMod.CurrentGameState;

    /// <summary>
    /// Use the games localization system (if available) to localize the text with ID <paramref name="localizedTextID"/>, or use an <paramref name="overrideText"/> replacement instead.
    /// </summary>
    /// <param name="localizedTextID">The ID to localize from inside the TextDatablock</param>
    /// <param name="forceOverrideText">If the ID lookup should be skipped and the <paramref name="overrideText"/> be used instead.</param>
    /// <param name="overrideText">Fallback text used if localization isn't available (R5 and below) or if it has been forced using <paramref name="forceOverrideText"/></param>
    /// <returns>Localized string</returns>
    public static string GetLocalizedTextSafe(uint localizedTextID, bool forceOverrideText = false, string overrideText = null)
    {
        var text = overrideText;

        if(Is.R6OrLater && !forceOverrideText)
        {
            text = Localization_Text_Get_R6Plus(localizedTextID);
        }

        return text;
    }

    /// <summary>
    /// Use the games localization system (if available) to localize the text with ID <paramref name="localizedTextID"/>, or use an <paramref name="overrideText"/> replacement instead.
    /// <br/>Additionally formats the text using <see cref="Utils.UsersafeFormat(string, string[])"/>
    /// </summary>
    /// <param name="localizedTextID">The ID to localize from inside the TextDatablock</param>
    /// <param name="forceOverrideText">If the ID lookup should be skipped and the <paramref name="overrideText"/> be used instead.</param>
    /// <param name="overrideText">Fallback text used if localization isn't available (R5 and below) or if it has been forced using <paramref name="forceOverrideText"/></param>
    /// <param name="formatArgs">Parameters to replace format literals with</param>
    /// <returns>Localized and formatted string</returns>
    public static string GetLocalizedTextSafeAndFormat(uint localizedTextID, bool forceOverrideText = false, string overrideText = null, params string[] formatArgs)
    {
        var text = GetLocalizedTextSafe(localizedTextID, forceOverrideText, overrideText);

        if (formatArgs == null || formatArgs == Array.Empty<string>())
            return text;

        return Utils.UsersafeFormat(text, formatArgs);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string Localization_Text_Get_R6Plus(uint id)
    {
#if IL2CPP
        return Localization.Text.Get(id);
#else
        return string.Empty;
#endif
    }

#if IL2CPP
    /// <summary>
    /// Cast an Il2Cpp type to another.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CastTo<T>(this Il2CppObjectBase value) where T : Il2CppObjectBase
    {
        return value.Cast<T>();
    }

    /// <summary>
    /// Try cast an Il2Cpp type to another.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T TryCastTo<T>(this Il2CppObjectBase value) where T : Il2CppObjectBase
    {
        return value.TryCast<T>();
    }

    /// <summary>
    /// Try cast an Il2Cpp type to another.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="castedValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool TryCastTo<T>(this Il2CppObjectBase value, out T castedValue) where T : Il2CppObjectBase
    {
        castedValue = value.TryCastTo<T>();
        return castedValue != null;
    }
#else
    public static T CastTo<T>(this object value)
    {
        return (T) value;
    }

    public static T TryCastTo<T>(this object value) where T : class
    {
        return value as T;
    }

    public static bool TryCastTo<T>(this object value, out T castedValue) where T : class
    {
        castedValue = value.TryCastTo<T>();
        return castedValue != null;
    }
#endif
}