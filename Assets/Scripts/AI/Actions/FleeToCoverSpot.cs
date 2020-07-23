using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeToCoverSpot : UtilityAction
{
    Enemy me;
    Landmark cover;

    private void Start()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");
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
}
