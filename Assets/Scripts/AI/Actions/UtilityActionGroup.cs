using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Contains similar actions
public class UtilityActionGroup : UtilityAction
{
    protected List<UtilityAction> subActions = new List<UtilityAction>();
    protected UtilityAction bestAction;


    public override bool Score(out int rank, out float weight)
    {
        rank = -10000;
        weight = 0.0f;
        if (subActions.Count == 0)
        {
            return false;
        }

        if (considerations.Count == 0)
        {
            rank = 0;
            weight = 1.0f;
            return true;
        }
        
        foreach (UtilityConsideration consideration in considerations)
        {
            float considerationWeight = consideration.Score();
            if (!Mathf.Approximately(considerationWeight, 0.0f))
            {
                weight += considerationWeight;
                rank = Mathf.Max(rank, (int)consideration.Rank());
            }
            else
            {
                return false;
            }
        }

        

        bestAction = subActions[0];
        foreach (UtilityAction subAction in subActions)
        {
            subAction.Tick();
            int subActionRank;
            float subActionWeight;
            if (subAction.Score(out subActionRank, out subActionWeight))
            {
                if (subActionRank > rank)
                {
                    bestAction = subAction;
                    rank = subActionRank;
                }
            }
        }

        bestAction.RepeatConsideration(ref rank);
        bestAction.CommitConsideration(ref rank);

        return true;
    }
}
