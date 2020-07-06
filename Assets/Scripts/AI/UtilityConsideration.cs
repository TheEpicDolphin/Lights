using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityConsideration
{
    protected UtilityRank baseRank;
    public UtilityConsideration(UtilityRank baseRank)
    {
        this.baseRank = baseRank;
    }

    public virtual bool Score(Dictionary<string, object> memory, out float weight)
    {
        weight = 1.0f;
        return true;
    }

    public virtual UtilityRank Rank()
    {
        return baseRank;
    }

}
