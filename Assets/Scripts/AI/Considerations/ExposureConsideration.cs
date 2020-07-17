using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposureConsideration : UtilityConsideration
{
    Enemy me;
    const float MAX_EXPOSURE_TIME = 5.0f;
    public ExposureConsideration(Enemy me, UtilityRank baseRank) : base(baseRank)
    {
        this.me = me;
    }

    public override float Score()
    {
        Player player = me.player;
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            float t = me.ExposedTime();
            float weight = Mathf.Min(t / MAX_EXPOSURE_TIME, 1.0f);
            return weight;
        }
        return 0.0f;
    }
}
