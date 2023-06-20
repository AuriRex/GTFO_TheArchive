using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArchive.Core.FeaturesAPI
{
    public static class FeatureGroups
    {
        public static Group Get(string name) => Group.Get(name);

        public class Group
        {
            private static readonly HashSet<Group> _allGroups = new HashSet<Group>();

            public static Group Get(string name)
            {
                return _allGroups.FirstOrDefault(g => g.Name == name);
            }

            public string Name { get; private set; }
            public bool InlineSettings { get; internal set; }
            public bool IsNewlyCreated { get; private set; } = true;

            public static Group GetOrCreate(string name, Action<Group> groupModification = null)
            {
                var group = _allGroups.FirstOrDefault(g => g.Name == name);

                if (group != null)
                    group.IsNewlyCreated = false;

                group ??= new Group(name);

                groupModification?.Invoke(group);

                return group;
            }

            private Group(string name)
            {
                Name = name;

                _allGroups.Add(this);
            }

            public override string ToString()
            {
                return Name;
            }

            public static implicit operator string(Group g) => g.Name; 
        }

        public static Group Accessibility { get; private set; } = Group.GetOrCreate("Accessibility");
        public static Group Backport { get; private set; } = Group.GetOrCreate("Backport");
        public static Group Cosmetic { get; private set; } = Group.GetOrCreate("Cosmetic");
        public static Group Dev { get; private set; } = Group.GetOrCreate("Developer");
        public static Group Fixes { get; private set; } = Group.GetOrCreate("Fixes");
        public static Group Hud { get; private set; } = Group.GetOrCreate("HUD / UI");
        public static Group LocalProgression { get; private set; } = Group.GetOrCreate("Local Progression"); // , group => group.InlineSettings = true
        public static Group Special { get; private set; } = Group.GetOrCreate("Misc");
        public static Group Presence { get; private set; } = Group.GetOrCreate("Discord / Steam Presence");
        public static Group Security { get; private set; } = Group.GetOrCreate("Security / Anti Cheat");
        public static Group QualityOfLife { get; private set; } = Group.GetOrCreate("Quality of Life");
    }
}
