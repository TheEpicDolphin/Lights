using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertConsideration : UtilityConsideration
{
    UtilityConsideration childConsideration;
    public InvertConsideration(UtilityConsideration childConsideration)
    {
        this.childConsideration = childConsideration;
    }

    public override float Score()
    {
        return 1.0f - this.childConsideration.Score();
    }
}
