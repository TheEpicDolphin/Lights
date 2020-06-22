﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAtDynamicTarget : UtilityAction
{
    float t0;
    Enemy me;
    Transform target;

    public AimAtDynamicTarget(Enemy me, Transform target)
    {
        this.t0 = Time.time;
        this.me = me;
        this.target = target;
    }

    public override float Score()
    {
        Vector2 myPos = me.transform.position;
        float dist = Vector2.Distance(myPos, target.position);
        
        return Mathf.Exp(-(Time.time - t0));
    }

    public override void Run()
    {
        /*
        Vector2 curHandDir = me.hand.GetHandDirection();
        Vector2 targetHandDir = (player.transform.position - me.transform.position).normalized;
        Vector2 interpolatedHandDir = Vector2.Lerp(curHandDir, targetHandDir, Time.deltaTime).normalized;
        me.hand.SetHandDirection(interpolatedHandDir);
        */
    }
}
