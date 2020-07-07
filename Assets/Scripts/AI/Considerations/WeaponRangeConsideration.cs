using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRangeConsideration : UtilityConsideration
{
    public WeaponRangeConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Enemy me, out float weight)
    {
        Vector2 target = me.GetShootingTarget();
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            float dist = Vector2.Distance(target, me.transform.position);
            float proximity = Mathf.Min(dist / firearm.GetRange(), 1.0f);

            weight = (1 / (1 + Mathf.Exp(50 * (proximity - 0.9f))));
            return true;
        }
        weight = 0.0f;
        return false;

    }
}
