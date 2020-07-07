using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Consider having multiple layers such as (Movement layer, LookAt layer, Attack layer, etc)
 * In each layer, actions are selected based on utility and each layer is independent from the rest.
 * There is also possibly a Script layer that takes over all of them
 * 
 */

public class UtilityAI
{
    public Dictionary<string, object> utilityMemory;
    private List<UtilityBucket> utilityBuckets;
    UtilityAction currentAction;

    public UtilityAI(List<UtilityBucket> utilityBuckets)
    {
        this.utilityMemory = new Dictionary<string, object>();
        this.utilityBuckets = utilityBuckets;
        this.currentAction = new Wait(0.0f);
    }

    public void RunOptimalAction()
    {
        UtilityBucket optimalBucket = new UtilityBucket("default");
        float bestScore = 0.0f;
        foreach (UtilityBucket bucket in utilityBuckets)
        {
            float score = bucket.EvaluatePriority(utilityMemory);
            if (score > bestScore)
            {
                bestScore = score;
                optimalBucket = bucket;
            }
        }
        optimalBucket.Run(utilityMemory);
    }

    public void AddMemory(string key, object obj)
    {
        utilityMemory[key] = obj;
    }
}
