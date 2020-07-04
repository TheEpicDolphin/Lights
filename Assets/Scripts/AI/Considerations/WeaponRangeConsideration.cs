using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRangeConsideration : UtilityConsideration
{
    public WeaponRangeConsideration(int rank) : base(rank)
    {

    }

    public override bool Score(Dictionary<string, object> memory, out float weight)
    {
        if (memory.ContainsKey("shooting_target") && memory.ContainsKey("me"))
        {
            Vector2 target = (Vector2)memory["shooting_target"];
            Enemy me = (Enemy)memory["me"];
            IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
            if (firearm != null)
            {
                float dist = Vector2.Distance(target, me.transform.position);
                float proximity = Mathf.Min(dist / firearm.GetRange(), 1.0f);

                weight = (1 / (1 + Mathf.Exp(50 * (proximity - 0.9f))));
                return true;
            }

        }
        weight = 0.0f;
        return false;

    }
}
