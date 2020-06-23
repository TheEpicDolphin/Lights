using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class ShootAtPlayer : UtilityDecision
{
    Player player;
    Enemy me;
    IFirearm firearm;
    public ShootAtPlayer(string name) : base(name)
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

        //Check if AI has gun equipped
        firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
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

        //Check if there is anything blocking line of sight from AI to player
        RaycastHit2D hit = Physics2D.Linecast(me.transform.position, player.transform.position);
        if (hit.collider.GetComponent<Player>() != null)
        {
            return 0.0f;
        }

        //Check if gun is ready to fire another round
        if (!firearm.ReadyToFire())
        {
            return 0.0f;
        }

        //TODO: Desire to shoot based on ammo remaining

        //Desire to shoot based on how close AI is aiming at player
        Transform barrelExit = firearm.GetBarrelExit();
        Plane2D los = new Plane2D(Vector2.Perpendicular(barrelExit.up), barrelExit.position);
        float aimError = Mathf.Min(los.DistanceToPoint(player.transform.position) / (1.5f * player.radius), 
                                    1.0f);

        //Desire to shoot based on proximity to target
        float dist = Vector2.Distance(player.transform.position, me.transform.position);
        float proximity = Mathf.Min(dist / firearm.GetRange(), 1.0f);

        float U = (1 - aimError) * (1 / (1 + Mathf.Exp(50 * (proximity - 0.9f))));
        return U;
    }

    public override UtilityAction Execute(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        Debug.Log("SHOOT");
        return new ShootFirearm(me);
    }
}
