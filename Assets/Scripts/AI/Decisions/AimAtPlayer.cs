using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class AimAtPlayer : UtilityDecision
{
    public AimAtPlayer(string name) : base(name)
    {
        considerations = new List<UtilityConsideration>()
        {
            new AccuracyConsideration(2),
            new WeaponRangeConsideration(2)
        };
    }

    public override void Execute(Dictionary<string, object> memory)
    {
        Vector2 target = (Vector2)memory["shooting_target"];
        Enemy me = (Enemy)memory["me"];
        Vector3 newAimingTarget = Vector3.Lerp(me.hand.AimTarget(), target, 5.0f * Time.deltaTime);
        me.hand.AimWeaponAtTarget(newAimingTarget);
    }
}
