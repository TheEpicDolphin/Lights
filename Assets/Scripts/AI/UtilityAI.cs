using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using AlgorithmUtils;


[XmlRoot("AI")]
public class UtilityAI
{
    public const string NAV = "Navigate";
    public const string AIM = "Aim";
    public const string SHOOT = "Shoot";
    public const string EXPOSE_FROM_COVER = "ExposeFromCover";
    public const string TAKE_COVER = "TakeCover";
    public const string STRAFE = "Strafe";

    [XmlElement(NAV, typeof(NavigateToStaticDestination))]
    [XmlElement(AIM, typeof(AimAtPlayer))]
    [XmlElement(SHOOT, typeof(ShootAtPlayer))]
    [XmlElement(EXPOSE_FROM_COVER, typeof(ExposeFromCover))]
    [XmlElement(TAKE_COVER, typeof(TakeCover))]
    [XmlElement(STRAFE, typeof(Strafe))]
    public UtilityAction[] actions;
    

    public static UtilityAI CreateFromXML()
    {
        ;
        UtilityAI uai = XMLOp.Deserialize<UtilityAI>(
            Path.Combine(Application.dataPath, "XML", "ai.xml"));
        Debug.Log(Path.Combine(Application.dataPath, "XML", "ai.xml"));
        Debug.Log(uai.actions.Length);
        return uai;
    }

    public void OptimalAction()
    {
        Dictionary<UtilityAction, KeyValuePair<int, float>> actionScoreMap = new Dictionary<UtilityAction, KeyValuePair<int, float>>();

        List<UtilityAction> possibleActions = new List<UtilityAction>();
        foreach (UtilityAction action in actions)
        {
            int rank;
            float weight;
            if (action.Score(this, out rank, out weight))
            {
                possibleActions.Add(action);
                actionScoreMap[action] = new KeyValuePair<int, float>(rank, weight);
            }
        }

        //Run until there are no more actions that don't conflict
        while (possibleActions.Count > 0)
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
            UtilityAction optimalAction = Algorithm.WeightedRandomSelection(highestScoringSubset);
            optimalAction.Execute(this);

            /* Intersect possibleActions and co-actions of the optimal action */
            List<UtilityAction> newPossibleActions = new List<UtilityAction>();
            foreach (UtilityAction possibleAction in possibleActions)
            {
                if (optimalAction.coActions.Contains(possibleAction.Name()))
                {
                    newPossibleActions.Add(possibleAction);
                }
            }
            possibleActions = newPossibleActions;
        }

        
    }

}
