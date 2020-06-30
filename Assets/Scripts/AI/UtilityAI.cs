using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAI
{
    public Dictionary<string, object> utilityMemory;
    private List<UtilityBucket> utilityBuckets;
    UtilityAction currentAction;

    public UtilityAI(List<UtilityBucket> utilityBuckets)
    {
        this.utilityMemory = new Dictionary<string, object>();
        this.utilityBuckets = utilityBuckets;
    }

    public void RunOptimalAction()
    {
        UtilityBucket optimalBucket = new UtilityBucket("default");
        float bestScore = 0.0f;
        foreach (UtilityBucket bucket in utilityBuckets)
        {
            float score = bucket.EvaluatePriority(utilityMemory);
            if (score < bestScore)
            {
                bestScore = score;
                optimalBucket = bucket;
            }
        }

        currentAction = optimalBucket.OptimalAction(utilityMemory);
        currentAction.Run();
    }

    public void AddMemory(string key, object obj)
    {
        utilityMemory[key] = obj;
    }
}
