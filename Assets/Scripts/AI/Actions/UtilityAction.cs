using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UtilityRank
{
    Low = 0,
    Medium = 1,
    High = 2,
    VeryHigh = 3
}

public class UtilityAction
{
    public HashSet<System.Type> coActions;

    protected List<UtilityConsideration> considerations = new List<UtilityConsideration>();

    /* Measured in frames */
    protected int lastExecutionFrame = 0;

    //Useful for actions to do necessary calculations beforehand
    public virtual void Tick()
    {

    }

    public virtual bool Score(out int rank, out float weight)
    {

        if(considerations.Count == 0)
        {
            rank = 0;
            weight = 1.0f;
            return true;
        }

        rank = -10000;
        weight = 0.0f;
        foreach (UtilityConsideration consideration in considerations)
        {
            float considerationWeight = consideration.Score();
            if(!Mathf.Approximately(considerationWeight, 0.0f))
            {
                weight += considerationWeight;
                rank = Mathf.Max(rank, (int)consideration.Rank());
            }
            else
            {
                return false;
            }
        }

        RepeatConsideration(ref rank);
        CommitConsideration(ref rank);

        return true;
    }

    public virtual void RepeatConsideration(ref int rank)
    {
        
    }

    public virtual void CommitConsideration(ref int rank)
    {

    }

    public virtual void Execute()
    {
        
    }


}
