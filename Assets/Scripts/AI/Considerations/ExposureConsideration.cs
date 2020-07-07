using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposureConsideration : UtilityConsideration
{
    public ExposureConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Dictionary<string, object> memory, out float weight)
    {
        if (memory.ContainsKey("me") && memory.ContainsKey("player"))
        {
            Enemy me = (Enemy)memory["me"];
            Player player = (Player)memory["player"];
            IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
            if (firearm != null)
            {
                float t = me.ExposedTime();
                float d = Vector2.Distance(me.transform.position, player.transform.position);

                weight = t/d;
                return true;
            }
        }
        weight = 0.0f;
        return false;
    }
}
