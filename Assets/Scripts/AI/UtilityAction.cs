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

    public virtual bool CheckPrerequisites(Dictionary<string, object> memory)
    {
        return false;
    }

    public virtual float Score(Dictionary<string, object> calculated)
    {
        return 0.0f;
    }

    //Returns cooldown time until next action can run. "Inertia"
    public virtual float Run(Dictionary<string, object> calculated)
    {
        return 0.0f;
    }
}
