using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class LookForCover : UtilityAction
{
    Player player;
    Enemy me;
    Collider2D[] coverColliders;
    float exposureTime;

    public LookForCover(string name) : base(name)
    {

    }

    public override bool CheckPrerequisites(Dictionary<string, object> memory)
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

        coverColliders = (Collider2D[])memory["cover_colliders"];

        return true;
    }

    public override float Score(Dictionary<string, object> calculated)
    {
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

    public override float Run(Dictionary<string, object> decisions, Dictionary<string, object> calculated)
    {
        List<Waypoint> validWaypoints = new List<Waypoint>();
        Vector2 playerDir = player.transform.position - me.transform.position;
        Vector2 midPoint = (player.transform.position + me.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);

        Collider2D[] waypointColliders = Physics2D.OverlapCircleAll(me.transform.position, 10.0f, 1 << 13);
        foreach(Collider2D waypointCollider in waypointColliders)
        {
            Waypoint waypoint = waypointCollider.GetComponent<Waypoint>();

            //Dont include waypoints that are closer to the player than to the enemy
            if (sepBoundary.GetSide(waypoint.transform.position))
            {
                validWaypoints.Add(waypoint);
            }
        }

        if (validWaypoints.Count == 0)
        {
            //There is no cover nearby. Prevent enemy from looking again any time soon
            return 4.0f;
        }

        List<KeyValuePair<float, Waypoint>> scoredWaypoints = new List<KeyValuePair<float, Waypoint>>();
        foreach (Waypoint waypoint in validWaypoints)
        {
            float score = 0.0f;

            //if waypoint is NOT in player's visibility cone, we give it a higher score
            if (!player.visibilityCone.OutlineContainsPoint(waypoint.transform.position))
            {
                score += 10.0f;
            }

            //Take into account distance from enemy to waypoint
            float dist = Vector2.Distance(waypoint.transform.position, me.transform.position);
            score += Mathf.Max(40.0f - dist, 0);

            //Take into account enemy's weapon range

            scoredWaypoints.Add(new KeyValuePair<float, Waypoint>(score, waypoint));
        }

        Waypoint optimalCoverSpot = Algorithm.WeightedRandomSelection<Waypoint>(scoredWaypoints);
        decisions["cover"] = optimalCoverSpot;
        //We found one. Don't try looking again anytime soon
        return 2.0f;
    }
}
