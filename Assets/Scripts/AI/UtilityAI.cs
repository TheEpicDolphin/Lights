using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAI
{
    public Dictionary<string, object> utilityMemory;
    private List<UtilityBucket> utilityBuckets;

    public UtilityAI(List<UtilityBucket> utilityBuckets)
    {
        this.utilityMemory = new Dictionary<string, object>();
        this.utilityBuckets = utilityBuckets;
    }

    public void RunOptimalAction()
    {
        utilityBuckets[0].RunOptimalAction(utilityMemory);
    }

    public void AddMemory(string key, object obj)
    {
        utilityMemory[key] = obj;
    }
}
