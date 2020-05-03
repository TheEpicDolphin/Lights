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

    public virtual float EvaluatePriority()
    {
        return 0.0f;
    }

    public void RunOptimalAction()
    {
        List<UtilityAction> sortedActions = utilityActions.OrderByDescending(action => action.Score()).ToList();
        List<UtilityAction> highestScoringSubset = sortedActions.GetRange(0, Mathf.Min(3, sortedActions.Count));

        float z = 0.0f;
        float[] cumScores = new float[highestScoringSubset.Count];
        for(int i = 0; i < cumScores.Length; i++)
        {
            UtilityAction action = highestScoringSubset[i];
            z += action.Score();
            cumScores[i] = z;
        }

        float x = Random.Range(0, z);
        for(int i = 0; i < cumScores.Length; i++)
        {
            if(x < cumScores[i])
            {
                //This action was randomly chosen out of the best for the AI to take
                highestScoringSubset[i].Run();
                return;
            }
        }

        //We shouldn't reach here unless bucket is empty
    }
}
