using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposureConsideration : UtilityConsideration
{
    public ExposureConsideration(int rank) : base(rank)
    {

    }

    public override bool Score(Dictionary<string, object> memory, out float weight)
    {
        if (memory.ContainsKey("me"))
        {
            Enemy me = (Enemy)memory["me"];
            IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
            if (firearm != null)
            {
                weight = me.exposure;
                return true;
            }

        }
        weight = 0.0f;
        return false;

    }
}
