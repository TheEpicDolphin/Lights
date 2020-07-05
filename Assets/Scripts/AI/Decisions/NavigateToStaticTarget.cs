using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateToStaticDestination : UtilityDecision
{
    public NavigateToStaticDestination(string name) : base(name)
    {
        considerations = new List<UtilityConsideration>()
        {
            new NavigationImportanceConsideration(1),
            new DestinationProximityConsideration(4),
            new DestinationExistsConsideration(1)
        };
    }

    public override void Execute(Dictionary<string, object> memory)
    {
        Enemy me = (Enemy)memory["me"];
        Vector2 dest = (Vector2)memory["destination"];
        me.NavigateTo(dest);
    }
}
