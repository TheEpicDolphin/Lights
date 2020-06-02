using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAction
{
    protected string name;
    
    public UtilityAction(string name)
    {
        this.name = name;
    }

    public virtual float Score(Dictionary<string, object> blackboard)
    {
        return 0.0f;
    }

    public virtual void Run(Dictionary<string, object> blackboard)
    {
        
    }
}
