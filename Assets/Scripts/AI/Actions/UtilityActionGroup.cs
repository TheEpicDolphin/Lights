using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Contains similar actions
public class UtilityActionGroup : UtilityAction
{
    protected List<UtilityAction> subActions = new List<UtilityAction>();
    protected UtilityAction bestAction;


    public override float Score()
    {
        float weight = 1.0f;

        foreach (UtilityConsideration consideration in considerations)
        {
            weight *= consideration.Score();
        }

        bestAction = subActions[0];
        float maxSubActionWeight = 0.0f;
        foreach (UtilityAction subAction in subActions)
        {
            subAction.Tick();
            float subActionWeight = subAction.Score();
            if(subActionWeight > maxSubActionWeight)
            {
                bestAction = subAction;
                maxSubActionWeight = subActionWeight;
            }
        }

        return weight * maxSubActionWeight;
    }
}
