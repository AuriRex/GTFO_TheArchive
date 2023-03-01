using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Backport
{
    [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFour)]
    public class MineFixBackport : Feature
    {
        public override string Name => "R5+ Mines";

        public override string Group => FeatureGroups.Backport;

        public override string Description => "Change explosion code to work like after the R5 update.\nMines = more effective\n\n<#f00>(Might cause desync!)</color>";

        private static readonly Collider[] _collidersNew = new Collider[300];
        private static readonly Collider[] _collidersOld = new Collider[50];

#if MONO
        private static FieldAccessor<DamageUtil, Collider[]> A_DamageUtil_s_tempColliders;

        public override void Init()
        {
            A_DamageUtil_s_tempColliders = FieldAccessor<DamageUtil, Collider[]>.GetAccessor("s_tempColliders");
        }

        public override void OnEnable()
        {
            A_DamageUtil_s_tempColliders.Set(null, _collidersNew);
        }

        public override void OnDisable()
        {
            A_DamageUtil_s_tempColliders.Set(null, _collidersOld);
        }
#else
        public override void OnEnable()
        {
            DamageUtil.s_tempColliders = _collidersNew;
        }

        public override void OnDisable()
        {
            DamageUtil.s_tempColliders = _collidersOld;
        }
#endif


    }
}
