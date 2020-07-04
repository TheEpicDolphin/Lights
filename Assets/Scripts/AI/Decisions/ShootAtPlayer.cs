using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class ShootAtPlayer : UtilityDecision
{
    public ShootAtPlayer(string name) : base(name)
    {
        considerations = new List<UtilityConsideration>()
        {
            new CanShootConsideration(4),
            new AccuracyConsideration(2),
            new WeaponRangeConsideration(2)
        };
    }

    public override void Execute(Dictionary<string, object> memory)
    {
        Enemy me = (Enemy)memory["me"];
        me.Attack();
    }
}
