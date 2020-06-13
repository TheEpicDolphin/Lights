using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class HideFromView : UtilityAction
{
    Player player;
    Enemy me;
    Collider2D[] coverColliders;
    float exposureTime;

    public HideFromView(string name) : base(name)
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

        coverColliders = (Collider2D[]) memory["cover_colliders"];

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

    public override void Run(Dictionary<string, object> calculated)
    {
        List<Vector2[]> blindSpots = player.visibilityCone.GetBlindSpotEdges();
        List<Vector2[]> closeBlindSpots = new List<Vector2[]>();

        Vector2 playerDir = player.transform.position - me.transform.position;
        Vector2 midPoint = (player.transform.position + me.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(playerDir.normalized, midPoint);

        foreach (Vector2[] blindSpot in blindSpots)
        {
            Vector2 v1 = blindSpot[0];
            Vector2 v2 = blindSpot[1];
            if (sepBoundary.GetSide(v1) && sepBoundary.GetSide(v2))
            {
                closeBlindSpots.Add(blindSpot);
            }
        }

        Vector2 obstructedPoint;
        
        if (!Physics2D.CircleCast(me.transform.position, me.radius, obstructedPoint, ))
        {
            me.NavigateTo(obstructedPoint);
            //Give this action a large amount of inertia

        }
        
    }
}
