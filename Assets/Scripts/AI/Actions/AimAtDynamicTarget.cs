using System.Collections;
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

        Vector3 curAimingDir = me.hand.AimingDirection();

        Vector3 diff = target.position - me.transform.position;
        Vector3 targetAimingDir = diff.normalized;
        Vector3 interpolatedAimingDir = Vector3.Slerp(curAimingDir, targetAimingDir, Time.deltaTime).normalized;

        Vector3 newAimingTarget = me.transform.position + diff.magnitude * interpolatedAimingDir;
        me.hand.AimWeaponAtTarget(newAimingTarget);
        
    }
}
