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

    public virtual float Score()
    {
        return 1.0f;
    }

    public virtual UtilityRank Rank()
    {
        return baseRank;
    }

}
