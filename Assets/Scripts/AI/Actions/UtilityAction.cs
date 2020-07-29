using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UtilityAction
{
    public HashSet<System.Type> coActions;

    protected List<UtilityConsideration> considerations = new List<UtilityConsideration>();

    /* Measured in frames */
    protected int lastExecutionFrame = 0;

    //Useful for actions to do necessary calculations beforehand
    public virtual void Tick()
    {

    }

    public virtual float Score()
    {
        float weight = 1.0f;
        foreach (UtilityConsideration consideration in considerations)
        {
            weight *= consideration.Score();
        }
        return weight;
    }

    public virtual void Execute()
    {
        
    }


}
