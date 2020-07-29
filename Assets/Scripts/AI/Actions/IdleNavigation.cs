using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Change name
public class IdleNavigation : UtilityAction
{
    Enemy me;
    
    public IdleNavigation(Enemy me)
    {
        this.me = me;

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
        //Don't move anywhere
    }
}
