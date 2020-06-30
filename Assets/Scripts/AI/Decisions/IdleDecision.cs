using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleDecision : UtilityDecision
{
    Player player;
    Enemy me;

    public IdleDecision(string name) : base(name)
    {

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

    public override float Score(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        if (!CheckPrerequisites(memory))
        {
            return 0.0f;
        }

        float U = Random.Range(0.0f, 1.0f);
        return U;
    }

    public override UtilityAction Execute(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        return new Wait(Random.Range(1.5f, 4.0f));
    }
}
