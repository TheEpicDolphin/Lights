using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationConsideration : UtilityConsideration
{
    public DestinationConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Enemy me, out float weight)
    {
        Vector2 dest = me.GetDestination();
        Vector2 curPos = me.transform.position;
        if (Vector2.Distance(curPos, dest) > 1e-2f)
        {
            weight = 1.0f;
            return true;
        }
        weight = 0.0f;
        return false;

    }
}
