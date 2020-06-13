using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

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
        List<Vector2[]> blindSpots = player.visibilityCone.GetBlindSpotEdges();
        List<Vector2[]> validBlindSpots = new List<Vector2[]>();

        Vector2 playerDir = player.transform.position - me.transform.position;
        Vector2 midPoint = (player.transform.position + me.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);
        
        foreach (Vector2[] blindSpot in blindSpots)
        {
            Vector2 v1 = blindSpot[0];
            Vector2 v2 = blindSpot[1];
            if (sepBoundary.GetSide(v1) && sepBoundary.GetSide(v2))
            {
                validBlindSpots.Add(blindSpot);
            }
        }

        if (validBlindSpots.Count == 0)
        {
            //There is no cover nearby. Prevent enemy from looking again any time soon
            return 4.0f;
        }

        Vector2 optimalBlindSpot = Vector2.zero;
        foreach (Vector2[] blindSpot in validBlindSpots)
        {

        }

        Vector2 myPos = me.transform.position;
        Vector2 coverDir = optimalBlindSpot - myPos;
        if (!Physics2D.CircleCast(me.transform.position, me.radius, coverDir.normalized, coverDir.magnitude))
        {
            decisions["cover_pos"] = optimalBlindSpot;
            //We found one. Don't try looking again anytime soon
            return 2.0f;
        }

        //Try again soon
        return 0.1f;
    }
}
