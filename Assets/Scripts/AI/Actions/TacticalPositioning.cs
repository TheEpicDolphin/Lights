using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalPositioning : UtilityActionGroup
{
    Enemy me;

    public TacticalPositioning(Enemy me)
    {
        this.me = me;
        considerations = new List<UtilityConsideration>()
        {
            
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer),
            typeof(ShootAtPlayer)
        };
    }

    public override void Tick()
    {
        Player player = me.player;
        List<Landmark> nearbyTacticalSpots = me.navMesh.GetLandmarksWithinRadius(me.transform.position,
                                        me.maxCoverDistance);
        subActions = new List<UtilityAction>();
        foreach (Landmark tacticalSpot in nearbyTacticalSpots)
        {
            subActions.Add(new MoveToTacticalSpot(me, tacticalSpot));
        }
    }

    public override void Execute()
    {
        bestAction?.Execute();
    }
}
