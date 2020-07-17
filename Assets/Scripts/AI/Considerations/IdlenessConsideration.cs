using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlenessConsideration : UtilityConsideration
{
    Enemy me;
    const float MAX_IDLE_TIME = 5.0f;
    public IdlenessConsideration(Enemy me, UtilityRank baseRank) : base(baseRank)
    {
        this.me = me;
    }

    public override float Score()
    {
        float t = me.IdleTime();
        float weight = 1.0f - Mathf.Min(t / MAX_IDLE_TIME, 1.0f);
        return weight;
    }
}
