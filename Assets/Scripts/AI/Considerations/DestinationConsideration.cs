using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationConsideration : UtilityConsideration
{
    Enemy me;
    public DestinationConsideration(Enemy me)
    {
        this.me = me;
    }

    public override float Score()
    {
        /*
        Vector2 dest = me.GetNavTarget().Position();
        Vector2 curPos = me.transform.position;
        if (Vector2.Distance(curPos, dest) > 1e-2f && me.GetNavTarget().IsValid())
        {
            return 1.0f;
        }
        */
        return 0.0f;

    }
}
