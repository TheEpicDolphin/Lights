using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertConsideration : UtilityConsideration
{
    UtilityConsideration consideration;
    public InvertConsideration(UtilityConsideration consideration)
    {
        this.consideration = consideration;
    }

    public override float Score()
    {
        return 1.0f - this.consideration.Score();
    }
}
