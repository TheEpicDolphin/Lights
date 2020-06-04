using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootFirearm : UtilityAction
{
    Player player;
    Enemy me;

    float range;
    public ShootFirearm(string name, float range) : base(name)
    {
        this.range = range;
    }

    public override bool CheckPrerequisites(Dictionary<string, object> memory)
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

        //Check if gun is equipped by me

        return true;
    }

    public override float Score(Dictionary<string, object> calculated)
    {
        float d = Vector3.Distance(player.transform.position, me.transform.position);
        //Get direction the enemy's gun is facing
        float U = Mathf.Max(range - d, 0.0f) / range;

        return U;
    }

    public override void Run(Dictionary<string, object> calculated)
    {
        me.Attack();
    }
}
