using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class TakeCover : UtilityDecision
{
    Player player;
    Enemy me;
    float maxExposureTime = 4.0f;

    public TakeCover(string name) : base(name)
    {

    }

    private bool CheckPrerequisites(Dictionary<string, object> memory)
    {
        if (memory.ContainsKey("player"))
        {
            player = (Player)memory["player"];
        }
        else
        {
            return false;
        }

        if (memory.ContainsKey("me"))
        {
            me = (Enemy)memory["me"];
        }
        else
        {
            return false;
        }

        return true;
    }

    public override float Score(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        if(!CheckPrerequisites(memory))
        {
            return 0.0f;
        }

        //TODO: Desire to hide based on ammo remaining


        //Desire to hide based on player's gun range
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
        {
            //If player does not have firearm, do not take cover
            return 0.0f;
        }
        float equippedFirearmRange = firearm.GetRange();
        float dist = Vector2.Distance(player.transform.position, me.transform.position);
        float proximity = Mathf.Min(dist / equippedFirearmRange, 1);

        //Desire to hide based on how long the enemy has been exposed in the player's FOV
        float exposure = Mathf.Min(me.DangerExposureTime() / maxExposureTime, 1);

        float U = 1 / (1 + Mathf.Exp(20 * (proximity - 0.85f))) * exposure;
        return U;
    }

    public override UtilityAction Execute(Dictionary<string, object> memory, Dictionary<string, object> calculated)
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
            return new Wait(0.0f);
        }

        Landmark currentCover = me.navMesh.GetLandmarkAt(me.transform.position);

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
                score += 1.0f;
            }

            scoredLandmarks.Add(new KeyValuePair<float, Landmark>(score, landmark));
        }

        Landmark optimalCoverSpot = Algorithm.WeightedRandomSelection(scoredLandmarks);
        if(optimalCoverSpot == currentCover)
        {
            //The preferred cover spot is still the current cover spot
            return new Wait(0.0f);
        }

        /* We found one. Don't try looking again anytime soon */
        return new NavigateToStaticTarget(me, optimalCoverSpot.p);
    }
}
