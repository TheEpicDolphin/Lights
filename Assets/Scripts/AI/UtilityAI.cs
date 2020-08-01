using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AlgorithmUtils;

public class UtilityAI
{
    public UtilityAction[] actions;
    const int MAX_CONCURRENT_ACTIONS = 5;

    public UtilityAI(UtilityAction[] actions)
    {
        this.actions = actions;
    }

    public void RunOptimalActions()
    {
        List<KeyValuePair<float, UtilityAction>> scoredActions = new List<KeyValuePair<float, UtilityAction>>();
        foreach (UtilityAction action in actions)
        {
            action.Tick();
            float weight = action.Score();
            if(weight > 0)
            {
                scoredActions.Add(new KeyValuePair<float, UtilityAction>(weight, action));
            }
        }
        scoredActions = scoredActions.OrderByDescending(s => s.Key).ToList();

        int t = 0;
        //Run until there are no more actions that don't conflict
        while (scoredActions.Count > 0 && t < MAX_CONCURRENT_ACTIONS)
        {
            /* First, get actions with highest rank */
            List<KeyValuePair<float, UtilityAction>> highestScoringSubset = new List<KeyValuePair<float, UtilityAction>>();
            float highestScore = scoredActions.First().Key;
            highestScoringSubset.Add(scoredActions.First());
            foreach (KeyValuePair<float, UtilityAction> scoredAction in scoredActions.Skip(1))
            {
                if(scoredAction.Key > 0.7f * highestScore)
                {
                    highestScoringSubset.Add(scoredAction);
                }
            }
            UtilityAction optimalAction = Algorithm.WeightedRandomSelection(highestScoringSubset);
            optimalAction.Execute();
            //Debug.Log(optimalAction.GetType());

            /* Intersect scoredActions and co-actions of the optimal action */
            List<KeyValuePair<float, UtilityAction>> newScoredActions = new List<KeyValuePair<float, UtilityAction>>();
            foreach (KeyValuePair<float, UtilityAction> scoredAction in scoredActions)
            {
                if (optimalAction.coActions.Contains(scoredAction.Value.GetType()))
                {
                    newScoredActions.Add(scoredAction);
                }
            }
            scoredActions = newScoredActions;

            t += 1;
        }
        //Debug.Log("====================");

        
    }

}
