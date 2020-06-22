using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AlgorithmUtils;

public class UtilityBucket
{
    string name;
    List<UtilityDecision> utilityDecisions;
    UtilityAction currentAction;

    public UtilityBucket(string name, List<UtilityDecision> decisions)
    {
        this.name = name;
        this.utilityDecisions = decisions;
        this.currentAction = new Wait(0.0f);
    }

    public virtual float EvaluatePriority(Dictionary<string, object> memory)
    {
        return 0.0f;
    }

    public void RunOptimalAction(Dictionary<string, object> memory)
    {
        Dictionary<string, object> calculated = new Dictionary<string, object>();

        List<KeyValuePair<float, UtilityDecision>> scoredDecisions = new List<KeyValuePair<float, UtilityDecision>>();
        float currentActionScore = currentAction.Score();
        foreach (UtilityDecision decision in utilityDecisions)
        {
            float decisionScore = decision.Score(memory, calculated);
            if(decisionScore > currentActionScore)
            {
                scoredDecisions.Add(new KeyValuePair<float, UtilityDecision>(decisionScore, decision));
            }
        }

        if(scoredDecisions.Count > 0)
        {
            scoredDecisions = scoredDecisions.OrderByDescending(action => action.Key).ToList();
            List<KeyValuePair<float, UtilityDecision>> highestScoringSubset = scoredDecisions.GetRange(0, Mathf.Min(3, scoredDecisions.Count));
            currentAction = Algorithm.WeightedRandomSelection(highestScoringSubset).Execute(memory, calculated);
        }

        currentAction.Run();
    }
}
