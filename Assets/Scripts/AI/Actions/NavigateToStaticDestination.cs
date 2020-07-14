using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateToStaticDestination : UtilityAction
{
    Enemy me;
    public NavigateToStaticDestination()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");

        considerations = new List<UtilityConsideration>()
        {
            new DestinationConsideration(me, UtilityRank.High),
        };
    }

    public override void Execute()
    {
        Vector2 dest = me.GetDestination();
        me.NavigateTo(dest);
    }

}
