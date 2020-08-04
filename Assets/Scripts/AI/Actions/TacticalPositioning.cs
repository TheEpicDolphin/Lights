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
        List<Vector2[]> pathsToNearbyTacticalPositions = me.navMesh.GetShortestPathsFromTo(me.transform.position,
                                                            me.GetTacticalPositioningCandidates());
        subActions = new List<UtilityAction>();
        foreach (Vector2[] path in pathsToNearbyTacticalPositions)
        {
            //TODO: tacticalSpot is type TacticalSpot instead of Vector2
            subActions.Add(new MoveToTacticalSpot(me, new TacticalSpot(me.transform.position, path)));
        }
    }

    public override void Execute()
    {
        bestAction?.Execute();
    }
}
