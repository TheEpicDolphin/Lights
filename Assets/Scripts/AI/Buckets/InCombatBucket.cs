using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InCombatBucket : UtilityBucket
{

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

    public override float EvaluatePriority(Dictionary<string, object> memory)
    {
        Enemy me = (Enemy)memory["me"];
        //utility goes up with more exposure
        float U = me.exposure;
        return U;
    }

    public override void Run(Dictionary<string, object> memory)
    {
        Player player = (Player)memory["player"];
        Enemy me = (Enemy)memory["me"];

        memory["shooting_target"] = player.transform.position;
        memory["target_radius"] = player.radius;

        
    }
}
