using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupiedConsideration : UtilityConsideration
{
    Enemy me;
    Vector2 tacticalSpot;
    public OccupiedConsideration(Enemy me, Vector2 tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;
    }

    public override float Score()
    {
        if (Physics2D.OverlapCircle(tacticalSpot, 0.75f * me.radius, 1 << 11))
        {
            return 0.0f;
        }
        else
        {
            return 1.0f;
        }
    }
}
