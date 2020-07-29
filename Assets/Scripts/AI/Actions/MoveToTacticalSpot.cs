using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTacticalSpot : UtilityAction
{
    Enemy me;
    Landmark tacticalSpot;

    public MoveToTacticalSpot(Enemy me, Landmark tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;

        considerations = new List<UtilityConsideration>()
        {
            new TacticalConsideration(me, tacticalSpot),
            new ExposureConsideration(me, tacticalSpot),
            new OccupiedConsideration(me, tacticalSpot),
            new PlayerWeaponRangeConsideration(me, tacticalSpot),
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer),
            typeof(ShootAtPlayer)
        };
    }

    public override void Execute()
    {
        me.NavigateTo(tacticalSpot.p);
    }

    /*
    public override void CommitConsideration(ref int rank)
    {
        if (lastExecutionFrame + 1 == Time.frameCount)
        {
            rank += 1;
        }
    }
    */

}
