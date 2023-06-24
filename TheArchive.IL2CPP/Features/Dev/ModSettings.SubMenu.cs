using CellMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Dev
{
    public partial class ModSettings
    {
        public class DynamicSubMenu : SubMenu
        {
            private readonly Action<DynamicSubMenu> _buildMenuAction;

            public DynamicSubMenu(string title, Action<DynamicSubMenu> buildMenuAction) : base(title)
            {
                _buildMenuAction = buildMenuAction;
            }

            public override bool Build()
            {
                ClearItems();
                _buildMenuAction?.Invoke(this);
                base.Build();
                HasBeenBuilt = false;
                return true;
            }

            public override void Show()
            {
                Build();
                base.Show();
            }

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
                    GameObject.Destroy(item.GameObject);
                }
                content.Clear();
            }
        }

        public class SubMenu
        {
            internal static Stack<SubMenu> openMenus = new Stack<SubMenu>();

            public SubMenu(string title)
            {
                Title = title;
                ScrollWindow = SettingsCreationHelper.CreateScrollWindow(title);

                AddToAllSettingsWindows(ScrollWindow);
            }

            public string Title { get; private set; }
            public bool HasBeenBuilt { get; protected set; }
            public float Padding { get; set; } = 5;
            public bool AddContentAsPersistent { get; set; } = false;
            public Transform WindowTransform => ScrollWindow.transform;
            public CM_ScrollWindow ScrollWindow { get; private set; }

            protected readonly List<SubMenuEntry> persistentContent = new List<SubMenuEntry>();
            protected readonly List<SubMenuEntry> content = new List<SubMenuEntry>();

            public bool AppendContent(GameObject go)
            {
                return AppendContent(new SubMenuEntry(go));
            }

            public virtual bool AppendContent(SubMenuEntry item)
            {
                if (HasBeenBuilt)
                    return false;

                if (AddContentAsPersistent)
                    persistentContent.Add(item);
                else
                    content.Add(item);

                return true;
            }

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

                HasBeenBuilt = true;
                return true;
            }

            public virtual void Show()
            {
                FeatureLogger.Debug($"Opening SubMenu \"{Title}\" ...");
                ShowScrollWindow(ScrollWindow);
                openMenus.Push(this);
            }

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

            public class SubMenuEntry
            {
                public GameObject GameObject { get; private set; }
                public iScrollWindowContent IScrollWindowContent { get; private set; }

                public SubMenuEntry(GameObject go)
                {
                    GameObject = go;
                    IScrollWindowContent = go.GetComponentInChildren<iScrollWindowContent>();

                    if (IScrollWindowContent == null) throw new ArgumentException($"Passed GameObject does not contain a Component inherriting from {nameof(iScrollWindowContent)} in its children!", nameof(go));
                }
            }
        }
    }
}
