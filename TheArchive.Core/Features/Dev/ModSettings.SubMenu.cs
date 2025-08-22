using CellMenu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Loader;
using TheArchive.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheArchive.Features.Dev;

public partial class ModSettings
{
    /// <summary>
    /// A dynamic submenu that gets re-built everytime it gets opened.
    /// </summary>
    public class DynamicSubMenu : SubMenu
    {
        private readonly Action<DynamicSubMenu> _buildMenuAction;

        /// <summary>
        /// Dynamic submenu constructor.
        /// </summary>
        /// <param name="title">The submenus title.</param>
        /// <param name="buildMenuAction">Menu build action - add your content here.</param>
        /// <param name="identifier">Identifier for this submenu.</param>
        public DynamicSubMenu(string title, Action<DynamicSubMenu> buildMenuAction, string identifier) : base(title, identifier)
        {
            _buildMenuAction = buildMenuAction;
        }

        /// <inheritdoc/>
        public override bool Build()
        {
            ClearItems();
            _buildMenuAction?.Invoke(this);
            base.Build();
            HasBeenBuilt = false;
            return true;
        }

        /// <inheritdoc/>
        public override void Show()
        {
            Build();
            base.Show();
        }

        /// <summary>
        /// Clear all non-persistent items.
        /// </summary>
        public void ClearItems()
        {
            foreach (var item in persistentContent)
            {
                // Setting new content destroys our old items so we unparent to have that not happen.
                // This works because the game code is iterating through the children of the gameobject that all items are parented to.
                item.GameObject.transform.SetParent(WindowTransform);
            }

            foreach (var item in content)
            {
                Object.Destroy(item.GameObject);
            }
            content.Clear();
        }
    }

    /// <summary>
    /// A mod settings sub menu.
    /// </summary>
    public class SubMenu : IDisposable
    {
        internal readonly string Identifier;
        internal static readonly Stack<SubMenu> openMenus = new();

        /// <summary>
        /// Creates a new submenu and adds it to the mod settings page.
        /// </summary>
        /// <param name="title">The submenu title.</param>
        /// <param name="identifier">Identifier for this submenu.</param>
        public SubMenu(string title, string identifier)
        {
            Identifier = identifier ?? title;
            Title = title;
            ScrollWindow = SettingsCreationHelper.CreateScrollWindow(title);

            AddToAllSettingsWindows(ScrollWindow);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (ScrollWindow == null)
                return;

            RemoveFromAllSettingsWindows(ScrollWindow);
            ScrollWindowClickAnyWhereListeners.Remove(ScrollWindow.GetInstanceID());
            ScrollWindow.SafeDestroyGO();
        }

        /// <summary>
        /// The title of the submenu.
        /// </summary>
        public string Title { get; private set; }
        
        /// <summary>
        /// If the submenu has been built.
        /// </summary>
        public bool HasBeenBuilt { get; protected set; }
        
        /// <summary>
        /// Padding between individual entries.
        /// </summary>
        public float Padding { get; set; } = 5;
        
        /// <summary>
        /// The transform of this scroll window.
        /// </summary>
        public Transform WindowTransform => ScrollWindow?.transform;
        
        /// <summary>
        /// The scroll window representing this submenu.
        /// </summary>
        public CM_ScrollWindow ScrollWindow { get; private set; }

        internal static Dictionary<int, List<iCellMenuCursorInputAnywhereItem>> ScrollWindowClickAnyWhereListeners = new();
        
        /// <summary>
        /// Content that should not be removed.
        /// </summary>
        protected readonly List<SubMenuEntry> persistentContent = new();
        
        /// <summary>
        /// Content that might be removed.
        /// </summary>
        protected readonly List<SubMenuEntry> content = new();

        private bool _addContentAsPersistent;

        /// <summary>
        /// Use a using block or dispose after you're done adding persistent content!
        /// </summary>
        /// <returns></returns>
        public PersistentContentAdditionToken GetPersistentContentAdditionToken()
        {
            return new PersistentContentAdditionToken(this);
        }

        /// <summary>
        /// Append a settings item to this submenu.
        /// </summary>
        /// <param name="go">Must contain a component implementing <c>iScrollWindowContent</c>!</param>
        /// <returns><c>True</c> if the content was successfully added.</returns>
        public bool AppendContent(GameObject go)
        {
            return AppendContent(new SubMenuEntry(go));
        }

        /// <summary>
        /// Append a settings item to this submenu.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns><c>True</c> if the content was successfully added.</returns>
        public virtual bool AppendContent(SubMenuEntry item)
        {
            if (HasBeenBuilt)
                return false;

            if (_addContentAsPersistent)
                persistentContent.Add(item);
            else
                content.Add(item);

            return true;
        }

        /// <summary>
        /// Build the submenu.
        /// </summary>
        /// <returns></returns>
        public virtual bool Build()
        {
            if (HasBeenBuilt)
                return false;

            var list = new List<iScrollWindowContent>();

            if (persistentContent.Count > 0)
                list.AddRange(persistentContent.Select(sme => sme.IScrollWindowContent));
            if (content.Count > 0)
                list.AddRange(content.Select(sme => sme.IScrollWindowContent));

            ScrollWindow.SetContentItems(list.ToIL2CPPListIfNecessary(), Padding);

            LoaderWrapper.StartCoroutine(UpdateCellMenuCursorItems());

            HasBeenBuilt = true;
            return true;
        }

        /// <summary>
        /// Show this submenu and push the previously showed one onto the stack.
        /// </summary>
        public virtual void Show()
        {
            FeatureLogger.Debug($"Opening SubMenu \"{Identifier}\" (Title: {Title}) ...");
            ShowScrollWindow(ScrollWindow);
            openMenus.Push(this);
        }

        /// <summary>
        /// Closes and re-opens the submenu.
        /// </summary>
        public virtual void Refresh()
        {
            Close();
            Show();
        }

        /// <summary>
        /// Close this submenu and show the previously opened one.
        /// </summary>
        public void Close()
        {
            if (openMenus.Count > 0)
            {
                openMenus.Pop();
            }

            if (openMenus.Count > 0)
            {
                openMenus.Pop().Show();
            }
            else
            {
                ShowMainModSettingsWindow(0);
            }
        }

        private IEnumerator UpdateCellMenuCursorItems()
        {
            yield return new WaitForEndOfFrame();
            var cursorItems = ScrollWindow.GetComponentsInChildren<iCellMenuCursorItem>();
            foreach (var cursorItem in cursorItems)
            {
                if (cursorItem.ID == 0)
                {
                    cursorItem.ID = CM_PageBase.NextCellItemID();
                    cursorItem.SetupCMItem();
                }
            }

            var cursorInputAnywhereItems = ScrollWindow.GetComponentsInChildren<iCellMenuCursorInputAnywhereItem>(true).ToList();
            cursorInputAnywhereItems.RemoveAt(0);
            ScrollWindowClickAnyWhereListeners[ScrollWindow.GetInstanceID()] = cursorInputAnywhereItems;
        }

        /// <summary>
        /// An entry for a submenu.
        /// </summary>
        public class SubMenuEntry
        {
            /// <summary>
            /// The gameobject of this entry.
            /// </summary>
            public GameObject GameObject { get; private set; }
            
            /// <summary>
            /// The iScrollWindowContent attached to this entries' gameobject.
            /// </summary>
            public iScrollWindowContent IScrollWindowContent { get; private set; }

            /// <summary>
            /// Submenu entry constructor.
            /// </summary>
            /// <param name="go">The gameobject to use.</param>
            /// <exception cref="ArgumentException">If there is no component implementing <c>iScrollWindowContent</c> on the passed gameobject.</exception>
            public SubMenuEntry(GameObject go)
            {
                GameObject = go;
                IScrollWindowContent = go.GetComponentInChildren<iScrollWindowContent>();

                if (IScrollWindowContent == null) throw new ArgumentException($"Passed GameObject does not contain a Component inheriting from {nameof(iScrollWindowContent)} in its children!", nameof(go));
            }
        }

        /// <summary>
        /// Used to add persistent content into a submenu.
        /// </summary>
        public class PersistentContentAdditionToken : IDisposable
        {
            private readonly SubMenu _subMenu;
            private readonly bool _previousAddAsPersistent;

            internal PersistentContentAdditionToken(SubMenu menu)
            {
                _subMenu = menu;
                _previousAddAsPersistent = menu._addContentAsPersistent;
                menu._addContentAsPersistent = true;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                _subMenu._addContentAsPersistent = _previousAddAsPersistent;
                GC.SuppressFinalize(this);
            }
        }
    }
}