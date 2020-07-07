using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlenessConsideration : UtilityConsideration
{
    public IdlenessConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Enemy me, out float weight)
    {
        float t = me.IdleTime();
        weight = t;
        return true;
    }
}
