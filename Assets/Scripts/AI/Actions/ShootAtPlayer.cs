using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class ShootAtPlayer : UtilityAction
{
    Enemy me;
    private void Start()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");

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
