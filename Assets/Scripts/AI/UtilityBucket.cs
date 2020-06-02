using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UtilityBucket
{
    string name;
    List<UtilityAction> utilityActions;

    public UtilityBucket(string name, List<UtilityAction> actions)
    {
        this.name = name;
        this.utilityActions = actions;
    }

    public virtual float EvaluatePriority(Dictionary<string, object> blackboard)
    {
        return 0.0f;
    }

    public void RunOptimalAction(Dictionary<string, object> blackboard)
    {
        List<UtilityAction> sortedActions = utilityActions.OrderByDescending(action => action.Score(blackboard)).ToList();
        List<UtilityAction> highestScoringSubset = sortedActions.GetRange(0, Mathf.Min(3, sortedActions.Count));

        float z = 0.0f;
        float[] cumScores = new float[highestScoringSubset.Count];
        for(int i = 0; i < cumScores.Length; i++)
        {
            UtilityAction action = highestScoringSubset[i];
            z += action.Score(blackboard);
            cumScores[i] = z;
        }

        float x = Random.Range(0, z);
        for(int i = 0; i < cumScores.Length - 1; i++)
        {
            if(x < cumScores[i])
            {
                //This action was randomly chosen out of the best for the AI to take
                highestScoringSubset[i].Run(blackboard);
                return;
            }
        }

        highestScoringSubset[cumScores.Length - 1].Run(blackboard);
    }
}
