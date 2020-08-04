using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupiedConsideration : UtilityConsideration
{
    Enemy me;
    TacticalSpot tacticalSpot;
    public OccupiedConsideration(Enemy me, TacticalSpot tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;
    }

    public override float Score()
    {
        Collider2D col = Physics2D.OverlapCircle(tacticalSpot.Position(), 0.75f * me.radius, 1 << 11);
        if (col != null && col.GetComponent<Enemy>() != me)
        {
            return 0.0f;
        }
        else
        {
            return 1.0f;
        }
    }
}
