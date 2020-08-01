using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRangeConsideration : UtilityConsideration
{
    Enemy me;
    public WeaponRangeConsideration(Enemy me)
    {
        this.me = me;
    }

    public override float Score()
    {
        Vector2 target = me.GetShootingTarget();
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            float dist = Vector2.Distance(target, me.transform.position);
            float proximity = Mathf.Min(dist / firearm.GetRange(), 1.0f);

            float weight = (1 / (1 + Mathf.Exp(50 * (proximity - 0.9f))));
            return weight;
        }
        return 0.0f;

    }
}
