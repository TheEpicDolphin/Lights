using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InCombatBucket : UtilityBucket
{
    Player player;
    Enemy me;

    public InCombatBucket(string name) : base(name)
    {
        this.utilityDecisions = new List<UtilityDecision>()
        {
            new Strafe("strafe"),
            new TakeCover("take_cover"),
            new AimAtPlayer("aim"),
            new ShootAtPlayer("shoot"),
            new IdleDecision("idle")
        };
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

        //utility goes up with more exposure
        float U = me.exposure;
        return U;
    }
}
