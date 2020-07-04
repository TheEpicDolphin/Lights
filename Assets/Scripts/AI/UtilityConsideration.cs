using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityConsideration
{
    int rank;
    public UtilityConsideration(int rank)
    {
        this.rank = rank;
    }

    public virtual bool Score(Dictionary<string, object> memory, out float weight)
    {
        weight = 1.0f;
        return true;
    }

    public virtual int Rank()
    {
        return rank;
    }

}
