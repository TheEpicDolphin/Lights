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
        Dictionary<UtilityAction, KeyValuePair<int, float>> actionScoreMap = new Dictionary<UtilityAction, KeyValuePair<int, float>>();

        List<UtilityAction> possibleActions = new List<UtilityAction>();
        foreach (UtilityAction action in actions)
        {
            int rank;
            float weight;
            if (action.Score(out rank, out weight))
            {
                possibleActions.Add(action);
                actionScoreMap[action] = new KeyValuePair<int, float>(rank, weight);
            }
        }

        int t = 0;
        //Run until there are no more actions that don't conflict
        while (possibleActions.Count > 0 && t < MAX_CONCURRENT_ACTIONS)
        {
            /* First, get actions with highest rank */
            List<KeyValuePair<float, UtilityAction>> scoredActions = new List<KeyValuePair<float, UtilityAction>>();
            int highestRank = int.MinValue;
            foreach (UtilityAction action in possibleActions)
            {
                KeyValuePair<int, float> rankWeight = actionScoreMap[action];
                int rank = rankWeight.Key;
                float weight = rankWeight.Value;
                if (rank > highestRank)
                {
                    scoredActions = new List<KeyValuePair<float, UtilityAction>>();
                    highestRank = rank;
                }
                if (rank == highestRank)
                {
                    scoredActions.Add(new KeyValuePair<float, UtilityAction>(weight, action));
                }
            }

            /* Among actions with highest rank, do a weighted random selection based on weight */
            scoredActions = scoredActions.OrderByDescending(action => action.Key).ToList();
            List<KeyValuePair<float, UtilityAction>> highestScoringSubset = scoredActions.GetRange(0, Mathf.Min(3, scoredActions.Count));
            foreach(KeyValuePair<float, UtilityAction> pair in highestScoringSubset)
            {
                Debug.Log(pair.Value.GetType());
                Debug.Log(pair.Key == 0.0f);
            }
            UtilityAction optimalAction = Algorithm.WeightedRandomSelection(highestScoringSubset);
            optimalAction.Execute();

            /* Intersect possibleActions and co-actions of the optimal action */
            List<UtilityAction> newPossibleActions = new List<UtilityAction>();
            foreach (UtilityAction possibleAction in possibleActions)
            {
                if (optimalAction.coActions.Contains(possibleAction.GetType()))
                {
                    newPossibleActions.Add(possibleAction);
                }
            }
            possibleActions = newPossibleActions;

            t += 1;
        }

        
    }

}
