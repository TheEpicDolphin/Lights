﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponRangeConsideration : UtilityConsideration
{
    public PlayerWeaponRangeConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Enemy me, out float weight)
    {
        Player player = me.player;
        IFirearm playerFirearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (playerFirearm != null)
        {
            float dist = Vector2.Distance(player.transform.position, me.transform.position);
            float proximity = Mathf.Min(dist / playerFirearm.GetRange(), 1.0f);

            weight = (1 / (1 + Mathf.Exp(50 * (proximity - 0.9f))));
            return true;
        }
        weight = 0.0f;
        return false;

    }
}