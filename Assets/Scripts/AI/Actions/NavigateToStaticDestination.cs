using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateToStaticDestination : UtilityAction
{
    
    public NavigateToStaticDestination()
    {
        considerations = new List<UtilityConsideration>()
        {
            new DestinationConsideration(UtilityRank.High),
        };
    }

    public override void Execute(Enemy me)
    {
        Vector2 dest = me.GetDestination();
        me.NavigateTo(dest);
    }

    public override string Name()
    {
        return UtilityAI.NAV;
    }
}
