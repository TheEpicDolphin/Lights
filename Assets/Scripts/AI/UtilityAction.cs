using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAction
{
    string name;
    
    public UtilityAction(string name)
    {
        this.name = name;
    }

    public virtual float Score()
    {
        return 0.0f;
    }

    public virtual void Run()
    {
        
    }
}
