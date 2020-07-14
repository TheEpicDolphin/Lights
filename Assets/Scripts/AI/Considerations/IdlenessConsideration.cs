using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlenessConsideration : UtilityConsideration
{
    Enemy me;
    public IdlenessConsideration(Enemy me, UtilityRank baseRank) : base(baseRank)
    {
        this.me = me;
    }

    public override bool Score(out float weight)
    {
        float t = me.IdleTime();
        weight = t;
        return true;
    }
}
