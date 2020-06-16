using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AlgorithmUtils;

public class UtilityBucket
{
    string name;
    List<UtilityAction> utilityActions;

    public UtilityBucket(string name, List<UtilityAction> actions)
    {
        this.name = name;
        this.utilityActions = actions;
    }

    public virtual float EvaluatePriority(Dictionary<string, object> memory)
    {
        return 0.0f;
    }

    public void RunOptimalAction(Dictionary<string, object> memory)
    {
        Dictionary<string, object> calculated = new Dictionary<string, object>();

        List<KeyValuePair<float, UtilityAction>> scoredActions = new List<KeyValuePair<float, UtilityAction>>();
        foreach(UtilityAction action in utilityActions)
        {
            scoredActions.Add(new KeyValuePair<float, UtilityAction>(action.Score(memory, calculated), action));
        }

        scoredActions = scoredActions.OrderByDescending(action => action.Key).ToList();
        List<KeyValuePair<float, UtilityAction>> highestScoringSubset = scoredActions.GetRange(0, Mathf.Min(3, scoredActions.Count));

        Algorithm.WeightedRandomSelection(highestScoringSubset).Run(memory, calculated);
    }
}
