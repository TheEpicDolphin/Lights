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
        List<Vector2> nearbyTacticalSpots = me.GetTacticalPositioningCandidates(20.0f);
        subActions = new List<UtilityAction>();
        foreach (Vector2 tacticalSpot in nearbyTacticalSpots)
        {
            subActions.Add(new MoveToTacticalSpot(me, tacticalSpot));
        }
    }

    public override void Execute()
    {
        bestAction?.Execute();
    }
}
