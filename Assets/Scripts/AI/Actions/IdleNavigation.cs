using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleNavigation : UtilityAction
{
    Enemy me;
    public IdleNavigation()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");

        considerations = new List<UtilityConsideration>()
        {
            new ExposureConsideration(me, UtilityRank.Medium),
            new IdlenessConsideration(me, UtilityRank.Medium),
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer),
            typeof(ShootAtPlayer)
            
        };
    }

    public override void Execute()
    {
        //do nothing
        me.SetDestination(me.transform.position);
    }
}
