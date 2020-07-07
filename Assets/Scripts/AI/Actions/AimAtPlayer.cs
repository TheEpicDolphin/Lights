using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class AimAtPlayer : UtilityAction
{
    public AimAtPlayer()
    {
        considerations = new List<UtilityConsideration>()
        {
            new AccuracyConsideration(UtilityRank.High),
            new WeaponRangeConsideration(UtilityRank.Medium)
        };
    }

    public override void Execute(Enemy me)
    {
        Vector2 target = me.GetShootingTarget();
        Vector3 newAimingTarget = Vector3.Lerp(me.hand.AimTarget(), target, 5.0f * Time.deltaTime);
        me.hand.AimWeaponAtTarget(newAimingTarget);
    }
}
