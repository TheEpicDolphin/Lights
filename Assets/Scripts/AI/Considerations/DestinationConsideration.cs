using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationConsideration : UtilityConsideration
{
    Enemy me;
    public DestinationConsideration(Enemy me, UtilityRank baseRank) : base(baseRank)
    {
        this.me = me;
    }

    public override bool Score(out float weight)
    {
        Vector2 dest = me.GetNavTarget().Position();
        Vector2 curPos = me.transform.position;
        if (Vector2.Distance(curPos, dest) > 1e-2f && me.GetNavTarget().IsValid())
        {
            weight = 1.0f;
            return true;
        }
        weight = 0.0f;
        return false;

    }
}
