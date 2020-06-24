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
        Mathf.Min(dist - 0.1f, 1.0f);

        float t = Time.time - t0;
        float maxT = 10.0f;
        float U = Mathf.Exp(-4 * (t / maxT));
        return U;
    }

    public override void Run()
    {
        //Return expected distance
        me.NavigateTo(target);
    }
}
