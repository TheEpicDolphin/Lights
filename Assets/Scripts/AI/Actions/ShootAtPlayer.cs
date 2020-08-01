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
            new CanShootConsideration(me),
            new AccuracyConsideration(me),
            new WeaponRangeConsideration(me)
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(TacticalPositioning),
            typeof(AimAtPlayer),
            typeof(Strafe)
        };
    }

    public override void Execute()
    {
        me.Attack();
    }
}
