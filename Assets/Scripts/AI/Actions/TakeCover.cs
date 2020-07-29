using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

public class TakeCover : UtilityActionGroup
{
    Enemy me;

    public TakeCover(Enemy me)
    {
        this.me = me;
        considerations = new List<UtilityConsideration>()
        {
            new PlayerWeaponRangeConsideration(me, UtilityRank.Medium),
            new ExposureConsideration(me, UtilityRank.High)
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer),
            typeof(ShootAtPlayer)
        };
    }

    public override void Tick()
    {
        Landmark currentClaimedCover = me.GetClaimedCover();

        Player player = me.player;
        List<Landmark> nearbyCoverSpots = me.navMesh.GetLandmarksWithinRadius(me.transform.position,
                                        me.maxCoverDistance);
        if(currentClaimedCover != null && !nearbyCoverSpots.Contains(currentClaimedCover))
        {
            nearbyCoverSpots.Add(currentClaimedCover);
        }
        subActions = new List<UtilityAction>();
        foreach (Landmark coverSpot in nearbyCoverSpots)
        {
            /* if landmark is NOT in player's visibility cone, it is a valid cover spot */
            if (!player.FOVContains(coverSpot.p))
            {
                subActions.Add(new FleeToCoverSpot(me, coverSpot));
            }
        }
    }

    public override void Execute()
    {
        bestAction?.Execute();
    }
}
