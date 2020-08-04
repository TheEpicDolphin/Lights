using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TacticalPositioning : UtilityAction
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

    public override void Execute()
    {
        TacticalSpot[] tacticalSpots = me.GetTacticalSpots();
        List<Vector2> tacticalSpotPositions = new List<Vector2>();
        foreach(TacticalSpot ts in tacticalSpots)
        {
            tacticalSpotPositions.Add(ts.transform.position);
        }

        Player player = me.player;
        List<Vector2[]> paths = me.navMesh.GetShortestPathsFromTo(me.transform.position,
                                                            tacticalSpotPositions);
        float bestScore = 0.0f;
        int best_i = 0;
        for(int i = 0; i < tacticalSpots.Length; i++)
        {
            float score = tacticalSpots[i].Score(paths[i]);
            if(score > bestScore)
            {
                bestScore = score;
                best_i = i;
            }
        }
        me.MoveTo(paths[best_i].Skip(1).First());
    }
}


