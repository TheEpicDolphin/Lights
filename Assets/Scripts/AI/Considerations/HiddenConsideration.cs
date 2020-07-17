using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenConsideration : UtilityConsideration
{
    Enemy me;
    const float MAX_HIDDEN_TIME = 10.0f;
    public HiddenConsideration(Enemy me, UtilityRank baseRank) : base(baseRank)
    {
        this.me = me;
    }

    public override float Score()
    {
        Player player = me.player;
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            float t = me.HiddenTime();
            float weight = Mathf.Min(t / MAX_HIDDEN_TIME, 1.0f);
            return weight;
        }
        return 0.0f;
    }
}
