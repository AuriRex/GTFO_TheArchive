using CellMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using GameData;
using TheArchive.Core.FeaturesAPI;
using System.Runtime.CompilerServices;
using TheArchive.Loader;
#if Unhollower
using UnhollowerBaseLib;
#endif
#if Il2CppInterop
using Il2CppInterop.Runtime.InteropTypes;
#endif

namespace TheArchive.Utilities
{
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
        public static List<T> ToSystemList<T>(this Il2CppSystem.Collections.Generic.List<T> il2List)
        {
            var list = new List<T>();

            foreach (var item in il2List)
            {
                list.Add(item);
            }

            return list;
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToIL2CPPListIfNecessary<T>(this List<T> list)
        {
            Il2CppSystem.Collections.Generic.List<T> il2List = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var item in list)
            {
                il2List.Add(item);
            }
            return il2List;
        }

        public static Il2CppSystem.Collections.Generic.List<T> NewListForGame<T>()
        {
            return new Il2CppSystem.Collections.Generic.List<T>();
        }
#else
        public static List<T> ToSystemList<T>(this List<T> list) => list;
        public static List<T> ToIL2CPPListIfNecessary<T>(this List<T> list) => list;
        public static List<T> NewListForGame<T>()
        {
            return new List<T>();
        }
#endif

        public static void ChangeColorTimedExpeditionButton(CM_TimedButton button, Color col) => ChangeColorOnAllChildren(button.transform, col, new string[] { "ProgressFill" });

        public static void ChangeColorCMItem(CM_Item item, Color idleColor, Color? hoverColor = null)
        {
            ChangeColorOnSelfAndAllChildren(item.transform, idleColor);
            List<Color> textColorsOut = new List<Color>();
            List<Color> textColorsOver = new List<Color>();
            var colorOut = idleColor.WithAlpha(.5f);
            var colorOver = hoverColor ?? idleColor.WithAlpha(1f);
#if IL2CPP
            item.m_spriteColorOrg = colorOut;
            item.m_spriteColorOut = colorOut;
            item.m_spriteColorOver = colorOver;
            if (item.m_textColorOrg != null)
                foreach (Color textCol in item.m_textColorOrg)
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

        public static void ChangeColorOnSelfAndAllChildren(Transform trans, Color col, IList<string> excludeNames = null, IgnoreMode mode = IgnoreMode.StartsWith, Action<Transform> extraModificationForEachChild = null)
        {
            ChangeColor(trans, col, excludeNames, mode, extraModificationForEachChild);
            ChangeColorOnAllChildren(trans, col, excludeNames, mode, extraModificationForEachChild);
        }

        public static void ChangeColorOnAllChildren(Transform trans, Color col, IList<string> excludeNames = null, IgnoreMode mode = IgnoreMode.StartsWith, Action<Transform> extraModificationForEachChild = null)
        {
            if (trans == null) return;
            trans.ForEachChildDo((child) => {
                ChangeColor(child?.transform, col, excludeNames, mode, extraModificationForEachChild);
            });
        }

        public static void ChangeColor(Transform trans, Color col, IList<string> excludeNames = null, IgnoreMode mode = IgnoreMode.StartsWith, Action<Transform> extraModificationForEachChild = null)
        {
            if (trans == null) return;
            if (col == null) return;
            if (excludeNames != null)
            {
                switch (mode)
                {
                    case IgnoreMode.Match:
                        if (excludeNames.Contains((string)trans.name))
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

        public static void RemoveAllEventHandlers<T>(string eventFieldName, object instance = null)
        {
#if IL2CPP
            typeof(T).GetProperty(eventFieldName, Utils.AnyBindingFlagss).SetValue(instance, null);
#else
            MonoUtils.RemoveAllEventHandlers<T>(eventFieldName, instance);
#endif
        }

        public static CM_Item AddCMItemEvents(this CM_Item item, Action<int> onButtonPress, Action<int, bool> onButtonHover = null)
        {
            if (onButtonPress != null)
                item.OnBtnPressCallback += onButtonPress;
            if (onButtonHover != null)
                item.OnBtnHoverChanged += onButtonHover;

            return item;
        }

        public static CM_Item SetCMItemEvents(this CM_Item item, Action<int> onButtonPress, Action<int, bool> onButtonHover = null)
        {
            if (item == null) throw new ArgumentNullException($"Parameter {nameof(item)} may not be null!");

            if(item.m_onBtnPress == null)
                item.m_onBtnPress = new UnityEngine.Events.UnityEvent();

#if IL2CPP
            item.OnBtnPressCallback = onButtonPress;
            if(onButtonHover != null)
                item.OnBtnHoverChanged = onButtonHover;
#else
            MonoUtils.RemoveAllEventHandlers<CM_Item>(nameof(CM_Item.OnBtnHoverChanged), item);
            MonoUtils.RemoveAllEventHandlers<CM_Item>(nameof(CM_Item.OnBtnPressCallback), item);

            item.OnBtnPressCallback += onButtonPress;
            if(onButtonHover != null)
                item.OnBtnHoverChanged += onButtonHover;
#endif

            return item;
        }

        public static CM_Item RemoveCMItemEvents(this CM_Item item, bool keepHover = false)
        {
            RemoveAllEventHandlers<CM_Item>(nameof(CM_Item.OnBtnPressCallback), item);
            if(!keepHover)
                RemoveAllEventHandlers<CM_Item>(nameof(CM_Item.OnBtnHoverChanged), item);

            return item;
        }

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
            if (trans == null) throw new ArgumentNullException($"Parameter {nameof(trans)} may not be null!");
            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                func?.Invoke(child);
                if (recursive)
                {
                    ForEachChildDo(child, func, recursive);
                }
            }
        }

        public static IEnumerable<Transform> DirectChildren(this Transform trans)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                yield return trans.GetChild(i);
            }
        }

        public static Transform GetChildWithExactName(this Transform trans, string name)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                if (child.name == name) return child;
            }
            return null;
        }

        public static T GetComponentOnSelfOrParents<T>(this Transform trans) where T : Component
        {
            if (trans == null) return null;

            T component = trans.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            return trans?.GetComponentOnParents<T>();
        }

        public static T GetComponentOnParents<T>(this Transform trans) where T : Component
        {
            if (trans == null) return null;

            var parent = trans.parent;

            T component = parent.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            return parent.parent?.GetComponentOnParents<T>();
        }

        public enum IgnoreMode
        {
            Match,
            StartsWith,
            EndsWith
        }

        public static bool TryGetPlayerByCharacterIndex(int id, out SNetwork.SNet_Player player)
        {
            try
            {
                player = SNetwork.SNet.Lobby.Players
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

        // https://gamedev.stackexchange.com/a/183962
        public static Bounds GetMaxBounds(this GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        public static bool SafeContains<T>(this IList<T> list, T item) where T : class, new()
        {
            if (LoaderWrapper.IsIL2CPPType(typeof(T)))
            {
#if IL2CPP
                foreach(var listItem in list)
                {
                    Il2CppSystem.Object il2Object = listItem as Il2CppSystem.Object;
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

        public static bool SafeIsBot(this SNetwork.SNet_Player player)
        {
            if (player == null)
                return false;
            if (Feature.Is.R6OrLater)
                return IsBotR6(player);
            return false;
        }

        public static bool IsFriend(this SNetwork.SNet_Player player)
        {
            if (player == null)
                return false;
            return SNetwork.SNet.Friends.TryGetFriend(player.Lookup, out _);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsBotR6(SNetwork.SNet_Player player)
        {
#if IL2CPP
            return player.IsBot;
#else
            return false;
#endif
        }

        private static readonly MethodAccessor<CellSoundPlayer> A_CellSoundPlayer_Post_sub_R5 = MethodAccessor<CellSoundPlayer>.GetAccessor("Post", new Type[] { typeof(uint) }, ignoreErrors: true);

        public static void SafePost(this CellSoundPlayer player, uint eventId, bool isGlobal = true)
        {
            if (Feature.Is.R6OrLater)
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
        private static PropertyAccessor<GameSetupDataBlock, uint> _rundownIdToLoad = PropertyAccessor<GameSetupDataBlock, uint>.GetAccessor("RundownIdToLoad");
        
        private static GameSetupDataBlock _setupBlock = null;
#endif

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
                if (_setupBlock == null)
                    _setupBlock = GameData.GameSetupDataBlock.GetBlock(1);
                blockToLoad = _rundownIdToLoad.Get(_setupBlock);
            }
#else
            blockToLoad = Globals.Global.RundownIdToLoad;
#endif
            if (blockToLoad != 0)
            {
                block = GameData.RundownDataBlock.GetBlock(blockToLoad);

                
                return true;
            }

            block = null;
            return false;
        }

        public static string GetDataBlockRundownTitle()
        {
            string text = null;
            if (TryGetRundownDataBlock(out var block))
            {
                text = Utils.StripTMPTagsRegex(block.StorytellingData.Title);
            }

            if(!ArchiveMod.IsPlayingModded && text != null)
            {
                text = text.Replace("TITLE:", "-");
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

        public static bool LocalPlayerIsInTerminal => FocusStateManager.CurrentState == eFocusState_ComputerTerminal;
        public static bool LocalPlayerIsInMap => FocusStateManager.CurrentState == eFocusState_Map;
        public static bool LocalPlayerIsDead => FocusStateManager.CurrentState == eFocusState_Dead;
        public static bool LocalPlayerIsInElevator => FocusStateManager.CurrentState == eFocusState_InElevator;
        public static bool LocalPlayerIsHacking => FocusStateManager.CurrentState == eFocusState_Hacking;

        public static eGameStateName GetGameState() => (eGameStateName) ArchiveMod.CurrentGameState;

        public static void RegisterOnGameStateChangedEvent(Action<eGameStateName> onGameStateChanged)
        {
#if IL2CPP
            ArchiveIL2CPPModule.OnGameStateChanged += onGameStateChanged;
#else
            ArchiveMONOModule.OnGameStateChanged += onGameStateChanged;
#endif
        }

        public static void UnregisterOnGameStateChangedEvent(Action<eGameStateName> onGameStateChanged)
        {
#if IL2CPP
            ArchiveIL2CPPModule.OnGameStateChanged -= onGameStateChanged;
#else
            ArchiveMONOModule.OnGameStateChanged -= onGameStateChanged;
#endif
        }

#if IL2CPP
        public static T CastTo<T>(this Il2CppObjectBase value) where T : Il2CppObjectBase
        {
            return value.Cast<T>();
        }

        public static T TryCastTo<T>(this Il2CppObjectBase value) where T : Il2CppObjectBase
        {
            return value.TryCast<T>();
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
#endif
    }
}
