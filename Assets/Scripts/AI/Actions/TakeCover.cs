using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class TakeCover : UtilityAction
{
    Enemy me;
    private void Start()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");
        considerations = new List<UtilityConsideration>()
        {
            new PlayerWeaponRangeConsideration(me, UtilityRank.Medium),
            new ExposureConsideration(me, UtilityRank.High),
            new RepeatConsideration()
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer),
            typeof(ShootAtPlayer)
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

            scoredLandmarks.Add(new KeyValuePair<float, Landmark>(score, landmark));
        }

        Landmark optimalCoverSpot = Algorithm.WeightedRandomSelection(scoredLandmarks);

        me.SetNavTarget(new CoverTarget(player, optimalCoverSpot.p));
        
    }
}
