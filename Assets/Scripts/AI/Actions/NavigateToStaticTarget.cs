using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateToStaticTarget : UtilityAction
{
    float t0;
    Enemy me;
    Vector2 target;

    public NavigateToStaticTarget(Enemy me, Vector2 target)
    {
        this.t0 = Time.time;
        this.me = me;
        this.target = target;
    }

    public override float Score()
    {
        Vector2 myPos = me.transform.position;
        float dist = Vector2.Distance(myPos, target);


        return Mathf.Exp(-(Time.time - t0));
    }

    public override void Run()
    {
        me.NavigateTo(target);
    }
}
