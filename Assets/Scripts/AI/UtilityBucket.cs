using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AlgorithmUtils;

public class UtilityBucket
{
    public string name;
    protected List<UtilityDecision> utilityDecisions;
    protected List<UtilityAction>[] layers;

    public UtilityBucket(string name)
    {
        this.name = name;
        this.utilityDecisions = new List<UtilityDecision>();
    }

    public virtual float EvaluatePriority(Dictionary<string, object> memory)
    {
        return 0.0f;
    }

    public void RunOptimalActions(Dictionary<string, object> memory)
    {

        
        //Pick best action in each layer
        foreach(List<UtilityAction> layer in layers)
        {
            List<KeyValuePair<float, UtilityDecision>> scoredDecisions = new List<KeyValuePair<float, UtilityDecision>>();
            int highestRank = -10000;
            foreach (UtilityDecision decision in utilityDecisions)
            {
                int rank;
                float weight;
                if(decision.Score(memory, out rank, out weight))
                {
                    if(rank > highestRank)
                    {
                        scoredDecisions = new List<KeyValuePair<float, UtilityDecision>>();
                        highestRank = rank;
                    }
                    if(rank == highestRank)
                    {
                        scoredDecisions.Add(new KeyValuePair<float, UtilityDecision>(weight, decision));
                        Debug.Log(decision.name + ": " + rank + ", " + weight);
                    }
                }
                
            }

            scoredDecisions = scoredDecisions.OrderByDescending(action => action.Key).ToList();
            List<KeyValuePair<float, UtilityDecision>> highestScoringSubset = scoredDecisions.GetRange(0, Mathf.Min(3, scoredDecisions.Count));
            UtilityAction optimalAction = Algorithm.WeightedRandomSelection(highestScoringSubset).Execute(memory);

            optimalAction.Run(memory);
        }

    }
}
