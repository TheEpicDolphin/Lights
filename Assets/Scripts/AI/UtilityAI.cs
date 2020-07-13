using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Data;


[XmlRoot("AI")]
public class UtilityAI
{
    public const string NAV = "Navigate";
    public const string AIM = "Aim";
    public const string SHOOT = "Shoot";
    public const string EXPOSE_FROM_COVER = "ExposeFromCover";
    public const string TAKE_COVER = "TakeCover";
    public const string STRAFE = "Strafe";

    private Dictionary<string, UtilityAction> map;

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
            Path.Combine(Application.dataPath, "ai.xml"));
        Debug.Log(uai.actions.Length);
        return uai;
    }

    public void OptimalAction()
    {
        //TODO: rank scoredActions by rank, and then randomly pick by weight. Eliminate 0 weight actions

        List<KeyValuePair<float, UtilityAction>> scoredActions = new List<KeyValuePair<float, UtilityAction>>();
        int highestRank = -10000;
        foreach (UtilityAction action in utilityActions)
        {
            int rank;
            float weight;
            if (action.Score(this, out rank, out weight))
            {
                if (rank > highestRank)
                {
                    scoredActions = new List<KeyValuePair<float, UtilityAction>>();
                    highestRank = rank;
                }
                if (rank == highestRank)
                {
                    scoredActions.Add(new KeyValuePair<float, UtilityAction>(weight, action));
                    Debug.Log(action.name + ": " + rank + ", " + weight);
                }
            }
        }

        List<UtilityAction> possibleActions = new List<UtilityAction>(actions);
        //Run until there are no more actions that don't conflict
        while (possibleActions.Count > 0)
        {
            scoredActions = scoredActions.OrderByDescending(action => action.Key).ToList();
            List<KeyValuePair<float, UtilityAction>> highestScoringSubset = scoredDecisions.GetRange(0, Mathf.Min(3, scoredDecisions.Count));
            UtilityAction optimalAction = Algorithm.WeightedRandomSelection(highestScoringSubset);
            optimalAction.Execute(this);

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
