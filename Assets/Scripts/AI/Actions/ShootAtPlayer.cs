using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAtPlayer : UtilityAction
{
    Enemy me;

    public ShootAtPlayer(Enemy me)
    {
        this.me = me;

        considerations = new List<UtilityConsideration>()
        {
            new CanShootConsideration(me, UtilityRank.High),
            new AccuracyConsideration(me, UtilityRank.Medium),
            new WeaponRangeConsideration(me, UtilityRank.Medium)
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(NavigateToStaticDestination),
            typeof(IdleNavigation),
            typeof(AimAtPlayer),
            typeof(TakeCover),
            typeof(Strafe)
        };
    }

    public override void Execute()
    {
        me.Attack();
    }
}
