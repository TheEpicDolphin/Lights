﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strafe : UtilityAction
{
    float maxStrafeDistance = 3.0f;

    public Strafe()
    {
        considerations = new List<UtilityConsideration>()
        {
            new ExposureConsideration(UtilityRank.Medium),
            new IdlenessConsideration(UtilityRank.Medium),
        };
    }

    public override float Score(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {

        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
        {
            //If player does not have firearm, do not strafe
            return 0.0f;
        }

        Vector2 playerDir = (player.transform.position - me.transform.position).normalized;

        float strafeDist = maxStrafeDistance;
        bool right = Random.Range(0.0f, 1.0f) > 0.5f;
        Vector2 strafeDir;
        if (right)
        {
            strafeDir = 2.0f * Vector2.Perpendicular(playerDir) +
                            Random.Range(-2.0f, 2.0f) * playerDir;
        }
        else
        {
            strafeDir = -2.0f * Vector2.Perpendicular(playerDir) +
                            Random.Range(-2.0f, 2.0f) * playerDir;
        }
        strafeDir.Normalize();

        Vector2 myPos = me.transform.position;
        RaycastHit2D hit = Physics2D.CircleCast(myPos, me.radius, strafeDir, maxStrafeDistance, (1 << 12));
        if(hit)
        {
            strafeDist = (hit.centroid - myPos).magnitude;
        }

        memory["strafe_target"] = myPos + strafeDist * strafeDir;
        float U = 0.5f * Mathf.Min(strafeDist / maxStrafeDistance, 1);
        return U;
    }

    public override void Execute(Enemy me)
    {
        Player player = me.player;
        bool right = Random.Range(0.0f, 1.0f) > 0.5f;
        Vector2 playerDir = (player.transform.position - me.transform.position).normalized;
        Vector2 strafeDir;
        if (right)
        {
            strafeDir = 2.0f * Vector2.Perpendicular(playerDir) +
                            Random.Range(-0.5f, 0.5f) * playerDir;
        }
        else
        {
            strafeDir = -2.0f * Vector2.Perpendicular(playerDir) +
                            Random.Range(-0.5f, 0.5f) * playerDir;
        }
        //me.MoveInDirection(strafeDir.normalized, Random.Range(0.5f, 1.0f) * me.speed);
    }
}