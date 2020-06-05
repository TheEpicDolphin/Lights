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

        //Check if AI has gun equipped
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
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

    public override float Score(Dictionary<string, object> calculated)
    {
        //Desire is measured as value in range [0, 1], where 0 is low desire and 1 is high desire.

        //TODO: Desire to shoot based on ammo remaining

        //Desire to shoot based on aiming direction
        Vector2 handDir = me.hand.GetHandDirection();
        Vector2 playerDir = player.transform.position - me.transform.position;
        float aim = 1 - (Vector2.Angle(handDir, playerDir) / 180.0f);

        //Desire to shoot based on proximity to target
        float dist = Vector3.Distance(player.transform.position, me.transform.position);
        float proximity = Mathf.Max(range - dist, 0.0f) / range;

        //Check if there is anything blocking player from enemy
        //TODO: Raycast

        float U = 1.0f;
        return U;
    }

    public override void Run(Dictionary<string, object> calculated)
    {
        me.Attack();
    }
}
