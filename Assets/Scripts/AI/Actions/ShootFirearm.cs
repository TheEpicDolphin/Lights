using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootFirearm : UtilityAction
{
    public ShootFirearm(string name) : base(name)
    {
        
    }

    public override float Score(Dictionary<string, object> blackboard)
    {
        Player player = (Player)blackboard["player"];
        if (!blackboard.ContainsKey("player"))
        {
            return -1.0f;
        }
        Enemy me = (Enemy)blackboard["me"];
        if (!blackboard.ContainsKey("me"))
        {
            return -1.0f;
        }

        return 0.0f;
    }

    public override void Run(Dictionary<string, object> blackboard)
    {
        Player player = (Player) blackboard["player"];
        Enemy me = (Enemy) blackboard["me"];

    }
}
