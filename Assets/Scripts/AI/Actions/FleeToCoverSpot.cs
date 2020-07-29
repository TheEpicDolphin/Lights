using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeToCoverSpot : UtilityAction
{
    Enemy me;
    Landmark cover;

    public FleeToCoverSpot(Enemy me, Landmark cover)
    {
        this.me = me;
        this.cover = cover;

        considerations = new List<UtilityConsideration>()
        {
            new CoverConsideration(me, cover, UtilityRank.High)
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer),
            typeof(ShootAtPlayer)
        };
    }

    public override void Execute()
    {
        me.NavigateTo(cover.p);
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
