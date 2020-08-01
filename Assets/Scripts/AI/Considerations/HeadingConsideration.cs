using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadingConsideration : UtilityConsideration
{
    Enemy me;
    Vector2 tacticalSpot;
    public HeadingConsideration(Enemy me, Vector2 tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;
    }

    public override float Score()
    {
        //TODO: Fix this when AI velocity has zero magnitude
        Vector2 curPos = me.transform.position;
        Vector2 curHeading = me.GetVelocity().normalized;
        Vector2 spotDirection = (tacticalSpot - curPos).normalized;
        float weight = Mathf.Clamp(Vector2.Dot(curHeading, spotDirection), 0.0f, 1.0f);
        return weight;
    }
}
