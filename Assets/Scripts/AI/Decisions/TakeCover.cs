using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class TakeCover : UtilityDecision
{
    Player player;
    Enemy me;

    public TakeCover(string name) : base(name)
    {
        considerations = new List<UtilityConsideration>()
        {
            new WeaponRangeConsideration(2),
            new ExposureConsideration(4)
        };
    }

    public override void Execute(Dictionary<string, object> memory)
    {
        float maxLandmarkDist = 15.0f;
        List<Landmark> nearbyLandmarks = me.navMesh.GetLandmarksWithinRadius(me.transform.position,
                                        maxLandmarkDist);
        List<Landmark> validLandmarks = new List<Landmark>();
        foreach (Landmark landmark in nearbyLandmarks)
        {
            /* if landmark is NOT in player's visibility cone, it is valid */
            if (!player.FOVContains(landmark.p))
            {
                validLandmarks.Add(landmark);
            }
        }

        
        if (validLandmarks.Count == 0)
        {
            /* There is no cover nearby. Decide again later */
            return;
        }

        Landmark currentCover = memory.ContainsKey("cover") ? (Landmark) memory["cover"] : 
                                me.navMesh.GetLandmarkAt(me.transform.position);

        Vector2 playerDir = player.transform.position - me.transform.position;
        Vector2 midPoint = (player.transform.position + me.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);

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

            if (landmark == currentCover)
            {
                //AI prefers to stay in place but may change cover if other options are
                //significantly better
                score += 3.0f;
            }

            scoredLandmarks.Add(new KeyValuePair<float, Landmark>(score, landmark));
        }

        Landmark optimalCoverSpot = Algorithm.WeightedRandomSelection(scoredLandmarks);

        memory["cover"] = optimalCoverSpot;
        memory["destination"] = optimalCoverSpot.p;
    }
}
