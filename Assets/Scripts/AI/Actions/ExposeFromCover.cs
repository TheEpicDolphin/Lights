﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class ExposeFromCover : UtilityAction
{
    Enemy me;
    float maxHideTime = 10.0f;

    public ExposeFromCover()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");
        considerations = new List<UtilityConsideration>()
        {
            new HiddenConsideration(me, UtilityRank.High)
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer)
        };
    }

    public override void Execute()
    {
        Player player = me.player;

        float maxLandmarkDist = 15.0f;
        List<Landmark> nearbyLandmarks = me.navMesh.GetLandmarksWithinRadius(me.transform.position,
                                        maxLandmarkDist);
        List<Landmark> validLandmarks = new List<Landmark>();
        foreach (Landmark landmark in nearbyLandmarks)
        {
            /* if AI can see player from landmark, it is valid */
            if (player.IsVisibleFrom(landmark.p))
            {
                validLandmarks.Add(landmark);
            }
        }

        if (validLandmarks.Count == 0)
        {
            /* There is no where for AI to expose itself nearby. Decide again later */
            return;
        }

        Vector2 playerDir = player.transform.position - me.transform.position;
        Vector2 midPoint = (player.transform.position + me.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);

        //TODO: discourage from choosing landmark that has path through player's FOV

        List<KeyValuePair<float, Landmark>> scoredLandmarks = new List<KeyValuePair<float, Landmark>>();
        foreach (Landmark landmark in validLandmarks)
        {
            /* Check if landmark is closer to AI than to player */
            float c = sepBoundary.SignedDistanceToPoint(landmark.p); 

            /* Take into account distance from AI to landmark */
            float dist = Vector2.Distance(landmark.p, me.transform.position);
            float proximity = Mathf.Min(dist / maxLandmarkDist, 1);

            /* TODO: Take into account AI's weapon range */

            /* Score landmark */
            float score = Mathf.Max(0, (1 - Mathf.Exp(-10 * c)) + (1.0f - proximity));

            scoredLandmarks.Add(new KeyValuePair<float, Landmark>(score, landmark));
        }

        Landmark optimalSpot = Algorithm.WeightedRandomSelection(scoredLandmarks);

        /* We found one. Don't try looking again anytime soon */
        me.SetNavTarget(new ExposedTarget(player, optimalSpot.p));
    }
}
