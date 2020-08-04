using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadingConsideration : UtilityConsideration
{
    Enemy me;
    TacticalSpot tacticalSpot;
    public HeadingConsideration(Enemy me, TacticalSpot tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;
    }

    public override float Score()
    {
        Vector2 curPos = me.transform.position;
        Vector2 curVelocity = me.GetVelocity();
        Vector2 theoreticalVelocity = me.VelocityToReachPosition(tacticalSpot.FirstPathPoint());
        float d = Vector2.Distance(curVelocity, theoreticalVelocity);
        float weight = 1.0f - Mathf.Max(0.6f, d / (2 * me.maxSpeed));
        return weight;
    }
}
