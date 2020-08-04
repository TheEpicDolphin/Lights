using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class TacticConsideration : UtilityConsideration
{
    Enemy me;
    TacticalSpot tacticalSpot;
    public TacticConsideration(Enemy me, TacticalSpot tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;
    }

    public override float Score()
    {
        //TODO: Split up below into two different considerations
        Player player = me.player;

        Vector2 playerDir = player.transform.position - me.transform.position;
        Vector2 midPoint = (player.transform.position + me.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);
        /* Check if landmark is closer to AI than to player */
        float c = sepBoundary.SignedDistanceToPoint(tacticalSpot.Position());

        /* Take into account distance from AI to landmark */
        float dist = tacticalSpot.Distance();
        /* higher proximity = tactical spot is closer to AI  */
        float proximity = 1.0f - Mathf.Clamp(dist / (1.5f * me.maxTacticalPositionRange), 0.0f, 0.5f);

        /* TODO: Take into account AI's weapon range */

        /* Score landmark */
        float score = Mathf.Max(0, (1 - Mathf.Exp(-10 * c)) * proximity);

        return score;

    }
}
