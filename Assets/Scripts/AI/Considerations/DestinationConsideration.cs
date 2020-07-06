using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationConsideration : UtilityConsideration
{
    public DestinationConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Dictionary<string, object> memory, out float weight)
    {
        if (memory.ContainsKey("me") && memory.ContainsKey("destination"))
        {
            Enemy me = (Enemy)memory["me"];
            Vector2 dest = (Vector2)memory["destination"];
            Vector2 curPos = me.transform.position;
            if(Vector2.Distance(curPos, dest) > 0.1f)
            {
                weight = 1.0f;
                return true;
            }
        }
        weight = 0.0f;
        return false;

    }
}
