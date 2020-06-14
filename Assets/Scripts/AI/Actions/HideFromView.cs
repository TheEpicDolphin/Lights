﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class HideFromView : UtilityAction
{
    Player player;
    Enemy me;
    Waypoint cover;

    public HideFromView(string name) : base(name)
    {
        
    }

    public override bool CheckPrerequisites(Dictionary<string, object> memory, Dictionary<string, object> decisions)
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

        if (memory.ContainsKey("cover_pos"))
        {
            cover = (Waypoint)memory["cover_pos"];
        }
        else
        {
            return false;
        }

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
        me.NavigateTo(cover.transform.position);
        return 0.0f;
    }
}
