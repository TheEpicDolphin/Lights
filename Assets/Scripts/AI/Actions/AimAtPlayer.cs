using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class AimAtPlayer : UtilityAction
{
    Enemy me;

    public AimAtPlayer(Enemy me)
    {
        this.me = me;

        considerations = new List<UtilityConsideration>()
        {
            new InaccuracyConsideration(me, UtilityRank.High),
            new WeaponRangeConsideration(me, UtilityRank.Medium)
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(ShootAtPlayer),
            typeof(FleeToCoverSpot),
            typeof(IdleNavigation),
            typeof(Strafe),
            typeof(TakeCover)
        };
    }

    public override void Execute()
    {
        Vector2 target = me.GetShootingTarget();
        Vector3 newAimingTarget = Vector3.Lerp(me.hand.AimTarget(), target, 5.0f * Time.deltaTime);
        me.hand.AimWeaponAtTarget(newAimingTarget);
    }
}
