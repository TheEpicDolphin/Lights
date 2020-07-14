using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposureConsideration : UtilityConsideration
{
    Enemy me;
    public ExposureConsideration(Enemy me, UtilityRank baseRank) : base(baseRank)
    {
        this.me = me;
    }

    public override bool Score(out float weight)
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
