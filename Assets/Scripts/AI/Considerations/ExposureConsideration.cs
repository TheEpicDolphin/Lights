﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposureConsideration : UtilityConsideration
{
    Enemy me;
    Landmark tacticalSpot;
    public ExposureConsideration(Enemy me, Landmark tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;
    }

    public override float Score()
    {
        Player player = me.player;
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            float tacticalSpotExposure = player.FOVContains(tacticalSpot.p) ? 1.0f : 0.0f;
            //AI seeks a change in exposure
            return Mathf.Abs(me.Exposure() - tacticalSpotExposure);
        }
        return 0.0f;
    }
}
