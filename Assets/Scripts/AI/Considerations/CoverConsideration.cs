using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class CoverConsideration : UtilityConsideration
{
    Enemy me;
    Landmark cover;
    public CoverConsideration(Enemy me, Landmark cover, UtilityRank rank) : base(rank)
    {
        this.me = me;
        this.cover = cover;
    }

    public override float Score()
    {
        float maxCoverDistance = me.maxCoverDistance;
        Player player = me.player;
        Vector2 playerDir = player.transform.position - me.transform.position;
        Vector2 midPoint = (player.transform.position + me.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);
        /* Check if landmark is closer to AI than to player */
        float c = sepBoundary.SignedDistanceToPoint(cover.p);

        /* Take into account distance from AI to landmark */
        float dist = Vector2.Distance(cover.p, me.transform.position);
        float proximity = Mathf.Min(dist / maxCoverDistance, 1);

        /* TODO: Take into account AI's weapon range */


        /* Score landmark */
        float score = Mathf.Max(0, (1 - Mathf.Exp(-10 * c)) + (1.0f - proximity));

        return score;

    }
}
