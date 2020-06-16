using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAtPlayer : UtilityAction
{
    Player player;
    Enemy me;

    float range;
    public AimAtPlayer(string name, float range) : base(name)
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

        //Check if AI has gun equipped
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
        {
            return false;
        }

        return true;
    }

    public override float Score(Dictionary<string, object> calculated)
    {
        //Desire to aim based on current aiming direction
        Vector2 handDir = me.hand.GetHandDirection();
        Vector2 playerDir = player.transform.position - me.transform.position;
        float aim = Vector2.Angle(handDir, playerDir) / 180.0f;

        //Desire to shoot based on proximity to target
        float dist = Vector3.Distance(player.transform.position, me.transform.position);
        float proximity = Mathf.Max(range - dist, 0.0f) / range;

        float U = 1.0f;
        return U;
    }

    public override float Run(Dictionary<string, object> calculated)
    {
        Vector2 curHandDir = me.hand.GetHandDirection();
        Vector2 targetHandDir = (player.transform.position - me.transform.position).normalized;
        Vector2 interpolatedHandDir = Vector2.Lerp(curHandDir, targetHandDir, Time.deltaTime).normalized;
        me.hand.SetHandDirection(interpolatedHandDir);
        return 1.0f;
    }
}
