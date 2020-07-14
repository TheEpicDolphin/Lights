using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class ShootAtPlayer : UtilityAction
{
    Enemy me;
    public ShootAtPlayer()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");

        considerations = new List<UtilityConsideration>()
        {
            new CanShootConsideration(me, UtilityRank.High),
            new AccuracyConsideration(me, UtilityRank.Medium),
            new WeaponRangeConsideration(me, UtilityRank.Medium)
        };
    }

    public override void Execute()
    {
        me.Attack();
    }
}
