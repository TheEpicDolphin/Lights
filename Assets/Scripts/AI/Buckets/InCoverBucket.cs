using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InCoverBucket : UtilityBucket
{
    Player player;
    Enemy me;

    public InCoverBucket(string name) : base(name)
    {
        this.utilityDecisions = new List<UtilityDecision>()
        {
            new ExposeFromCover("expose"),
            new ChangeCover("change_cover"),
            new IdleDecision("idle")
        };
        this.currentAction = new Wait(0.0f);
    }

    private bool CheckPrerequisites(Dictionary<string, object> memory)
    {
        if (memory.ContainsKey("player"))
        {
            player = (Player)memory["player"];
        }
        else
        {
            return false;
        }

        if (memory.ContainsKey("me"))
        {
            me = (Enemy)memory["me"];
        }
        else
        {
            return false;
        }

        return true;
    }

    public override float EvaluatePriority(Dictionary<string, object> memory)
    {
        if (!CheckPrerequisites(memory))
        {
            return 0.0f;
        }

        //utility goes up with less exposure
        float U = 1 - me.exposure;
        return U;
    }
}
