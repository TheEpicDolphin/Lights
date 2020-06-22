using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAtPlayer : UtilityDecision
{
    Player player;
    Enemy me;
    IFirearm firearm;

    float range;
    public ShootAtPlayer(string name, float range) : base(name)
    {
        this.range = range;
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

        //Check if AI has gun equipped
        firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
        {
            return false;
        }

        //Check if gun is ready to fire another round
        if (!firearm.ReadyToFire())
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

        //Desire is measured as value in range [0, 1], where 0 is low desire and 1 is high desire.

        //TODO: Desire to shoot based on ammo remaining

        //Desire to shoot based on how close AI is aiming at player
        //Vector2 handDir = me.hand.GetHandDirection();
        //Vector2 playerDir = player.transform.position - me.transform.position;
        //float aim = 1 - (Vector2.Angle(handDir, playerDir) / 180.0f);

        //Desire to shoot based on proximity to target
        float dist = Vector2.Distance(player.transform.position, me.transform.position);
        float proximity = Mathf.Max(range - dist, 0.0f) / range;

        float U = 1.0f;
        return U;
    }

    public override UtilityAction Execute(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        return new ShootFirearm();
    }
}
