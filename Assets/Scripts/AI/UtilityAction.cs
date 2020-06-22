using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAction
{
    public UtilityAction()
    {

    }

    /*
     * Perform weight decay here
     */
    public virtual float Score()
    {
        return 0.0f;
    }

    public virtual void Run()
    {
        
    }
}
