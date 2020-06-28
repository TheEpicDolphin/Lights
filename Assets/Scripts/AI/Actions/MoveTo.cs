using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTo : UtilityAction
{
    float t0;
    Enemy me;
    Vector2 target;
    float maxT;

    public MoveTo(Enemy me, Vector2 target)
    {
        this.t0 = Time.time;
        this.me = me;
        this.target = target;
        this.maxT = Vector2.Distance(me.transform.position, target) / me.speed;
    }

    public override float Score()
    {
        Vector2 myPos = me.transform.position;
        float dist = Vector2.Distance(myPos, target);
        Mathf.Min(dist - 0.1f, 1.0f);

        float t = Time.time - t0;
        float U = 1 / (1 + Mathf.Exp(50 * (t/maxT - 0.95f)));
        return U;
    }

    public override void Run()
    {
        //Return expected distance
        me.MoveTo(target);
    }
}
