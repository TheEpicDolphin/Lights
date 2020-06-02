using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAI
{
    public Dictionary<string, object> utilityBlackboard;
    private List<UtilityBucket> utilityBuckets;

    public UtilityAI(List<UtilityBucket> utilityBuckets)
    {
        this.utilityBlackboard = new Dictionary<string, object>();
        this.utilityBuckets = utilityBuckets;
    }


}
