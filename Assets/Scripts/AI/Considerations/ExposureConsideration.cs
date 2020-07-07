using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposureConsideration : UtilityConsideration
{
    public ExposureConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Enemy me, out float weight)
    {
        Player player = me.player;
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            float t = me.ExposedTime();
            float d = Vector2.Distance(me.transform.position, player.transform.position);

            weight = t / d;
            return true;
        }
        weight = 0.0f;
        return false;
    }
}
