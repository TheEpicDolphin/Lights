using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : UtilityAction
{
    Enemy me;
    public Idle()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");

        considerations = new List<UtilityConsideration>()
        {
            new ExposureConsideration(me, UtilityRank.Medium),
            new IdlenessConsideration(me, UtilityRank.Medium),
        };
    }

    public override void Execute()
    {
        //do nothing
        me.SetDestination(me.transform.position);
    }
}
