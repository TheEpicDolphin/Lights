using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityDecision
{
    protected string name;

    public UtilityDecision(string name)
    {
        this.name = name;
    }

    public virtual float Score(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        return 0.0f;
    }

    //Returns cooldown time until next decision can run. "Inertia"
    public virtual UtilityAction Execute(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        return new Wait(0.0f);
    }
}
