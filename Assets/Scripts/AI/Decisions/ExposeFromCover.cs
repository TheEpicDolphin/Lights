using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class ExposeFromCover : UtilityDecision
{
    Player player;
    Enemy me;
    float maxHideTime = 7.0f;

    public ExposeFromCover(string name) : base(name)
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
        if (!CheckPrerequisites(memory))
        {
            return 0.0f;
        }

        //No need to expose from cover when already exposed
        if (player.IsVisibleFrom(me.transform.position))
        {
            return 0.0f;
        }

        //TODO: Desire to hide based on ammo remaining

        //Desire to hide based on proximity to target
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if(firearm == null)
        {
            //Expose to attack player
            return 1.0f;
        }
        float equippedFirearmRange = firearm.GetRange();
        float dist = Vector2.Distance(player.transform.position, me.transform.position);
        float proximity = Mathf.Min(dist / equippedFirearmRange, 1);

        //Desire to hide based on how long the enemy has been hiding from player's FOV
        float exposure = Mathf.Max(me.HiddenTime() / maxHideTime, 1);

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
            /* if AI can see player from landmark, it is valid */
            if (player.IsVisibleFrom(landmark.p))
            {
                validLandmarks.Add(landmark);
            }
        }

        if (validLandmarks.Count == 0)
        {
            /* There is no where for AI to expose itself nearby. Decide again later */
            return new Wait(0.0f);
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

        Debug.Log(scoredLandmarks.Count);
        Landmark optimalCoverSpot = Algorithm.WeightedRandomSelection(scoredLandmarks);

        /* We found one. Don't try looking again anytime soon */
        return new NavigateToStaticTarget(me, optimalCoverSpot.p);
    }
}
