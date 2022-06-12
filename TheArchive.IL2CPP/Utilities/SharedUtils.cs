using CellMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

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
            foreach (var textCol in A_CM_Item_m_textColorOrg.Get(item))
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
            trans.ForEachChildDo((child) => {
                if(excludeNames != null)
                {
                    switch (mode)
                    {
                        case IgnoreMode.Match:
                            if (excludeNames.Contains(child.name))
                                return;
                            break;
                        case IgnoreMode.StartsWith:
                            if (excludeNames.Any(s => child.name.StartsWith(s)))
                                return;
                            break;
                        case IgnoreMode.EndsWith:
                            if (excludeNames.Any(s => child.name.EndsWith(s)))
                                return;
                            break;
                    }
                }
                var spriteRenderer = child.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = col;
                }
                var tmp = child.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.color = col;
                }
                extraModificationForEachChild?.Invoke(child);
            });
        }

        public static void ChangeColor(Transform trans, Color col, IList<string> excludeNames = null, IgnoreMode mode = IgnoreMode.StartsWith, Action<Transform> extraModificationForEachChild = null)
        {
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
            typeof(T).GetProperty(eventFieldName, Core.ArchivePatcher.AnyBindingFlags).SetValue(instance, null);
#else
            MonoUtils.RemoveAllEventHandlers<T>(eventFieldName, instance);
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

    }
}
