﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class LookForCover : UtilityAction
{
    Player player;
    Enemy me;
    float exposureTime;

    public LookForCover(string name) : base(name)
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

        //Check if AI has gun equipped
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
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

        //Desire is measured as value in range [0, 1], where 0 is low desire and 1 is high desire.

        //TODO: Desire to hide based on ammo remaining


        //Desire to hide based on proximity to target
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        float equippedFirearmRange = firearm.GetRange();
        float dist = Vector3.Distance(player.transform.position, me.transform.position);
        float proximity = 1 - Mathf.Max(equippedFirearmRange - dist, 0.0f) / equippedFirearmRange;

        //Desire to hide based on whether enemy is inside player's visibility cone
        float exposure = 0.0f;
        bool visibility = player.visibilityCone.OutlineContainsPoint(me.transform.position);
        if (visibility)
        {
            exposure = 0.5f;
        }

        float U = 1.0f;
        return U;
    }

    public override float Run(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        List<Landmark> validLandmarks = me.navMesh.GetLandmarksWithinRadius(me.transform.position, 15.0f);
        if (validLandmarks.Count == 0)
        {
            //There is no cover nearby. Prevent enemy from looking again any time soon
            return 4.0f;
        }

        Vector2 playerDir = player.transform.position - me.transform.position;
        Vector2 midPoint = (player.transform.position + me.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);

        List<KeyValuePair<float, Landmark>> scoredLandmarks = new List<KeyValuePair<float, Landmark>>();
        foreach (Landmark landmark in validLandmarks)
        {
            float score = 0.0f;

            //Check if landmark is closer to AI than to player
            if (sepBoundary.GetSide(landmark.p))
            {
                score += 10.0f;
            }

            //if waypoint is NOT in player's visibility cone, we give it a higher score
            if (!player.visibilityCone.OutlineContainsPoint(landmark.p))
            {
                score += 10.0f;
            }

            //Take into account distance from AI to landmark
            float dist = Vector2.Distance(landmark.p, me.transform.position);
            score += Mathf.Max(40.0f - dist, 0);

            //Take into account enemy's weapon range

            scoredLandmarks.Add(new KeyValuePair<float, Landmark>(score, landmark));
        }

        Landmark optimalCoverSpot = Algorithm.WeightedRandomSelection<Landmark>(scoredLandmarks);
        memory["cover"] = optimalCoverSpot;
        //We found one. Don't try looking again anytime soon
        return 2.0f;
    }
}
