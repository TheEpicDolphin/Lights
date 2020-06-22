using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wait : UtilityAction
{
    float delay;
    public Wait(float delay)
    {
        this.delay = delay;
    }

    public override float Score()
    {
        delay -= Time.deltaTime;
        if(delay < 0)
        {
            return 0.0f;
        }

        return 1.0f;
    }

    public override void Run()
    {
        //Do nothing
    }
}
