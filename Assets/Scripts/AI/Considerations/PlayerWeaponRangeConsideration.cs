using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponRangeConsideration : UtilityConsideration
{
    Enemy me;
    Vector2 tacticalSpot;
    public PlayerWeaponRangeConsideration(Enemy me, Vector2 tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;
    }

    public override float Score()
    {
        Player player = me.player;
        IFirearm playerFirearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (playerFirearm != null)
        {
            float dist = Vector2.Distance(player.transform.position, tacticalSpot);
            float proximity = Mathf.Min(dist / playerFirearm.GetRange(), 1.0f);

            float weight = (1 / (1 + Mathf.Exp(50 * (proximity - 0.9f))));
            return weight;
        }
        return 0.0f;

    }
}
