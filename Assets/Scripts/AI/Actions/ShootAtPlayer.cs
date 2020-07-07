using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class ShootAtPlayer : UtilityAction
{
    public ShootAtPlayer()
    {
        considerations = new List<UtilityConsideration>()
        {
            new CanShootConsideration(UtilityRank.High),
            new AccuracyConsideration(UtilityRank.Medium),
            new WeaponRangeConsideration(UtilityRank.Medium)
        };
    }

    public override void Execute(Enemy me)
    {
        me.Attack();
    }
}
