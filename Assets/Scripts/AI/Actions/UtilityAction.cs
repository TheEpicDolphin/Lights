using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;


public enum UtilityRank
{
    Low = 0,
    Medium = 1,
    High = 2
}

public class UtilityAction
{
    [XmlElement("coaction")]
    public HashSet<string> coActions;

    //Add considerations for XML later
    protected List<UtilityConsideration> considerations = new List<UtilityConsideration>();


    public bool Score(Enemy me, out int rank, out float weight)
    {
        if(considerations.Count == 0)
        {
            rank = 0;
            weight = 1.0f;
            return true;
        }

        rank = -10000;
        weight = 0.0f;
        foreach (UtilityConsideration consideration in considerations)
        {
            float considerationWeight;
            if (consideration.Score(me, out considerationWeight))
            {
                weight += considerationWeight;
                rank = Mathf.Max(rank, consideration.Rank());
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public virtual void Execute(Enemy me)
    {
        
    }

    public virtual string Name()
    {
        return "";
    }

}
