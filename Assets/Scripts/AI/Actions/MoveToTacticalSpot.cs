using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTacticalSpot : UtilityAction
{
    Enemy me;

    //TODO: Change this to TacticalSpot
    Vector2 tacticalSpot;

    public MoveToTacticalSpot(Enemy me, Vector2 tacticalSpot)
    {
        this.me = me;
        this.tacticalSpot = tacticalSpot;

        considerations = new List<UtilityConsideration>()
        {
            new TacticConsideration(me, tacticalSpot),
            new ExposureConsideration(me, tacticalSpot),
            new OccupiedConsideration(me, tacticalSpot),
            new HeadingConsideration(me, tacticalSpot),
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer),
            typeof(ShootAtPlayer)
        };
    }

    public override void Execute()
    {
        me.NavigateTo(tacticalSpot);
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
