using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlenessConsideration : UtilityConsideration
{
    public IdlenessConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Dictionary<string, object> memory, out float weight)
    {
        if (memory.ContainsKey("me"))
        {
            Enemy me = (Enemy)memory["me"];
            float t = me.IdleTime();


        }
        weight = 0.0f;
        return false;

    }
}
