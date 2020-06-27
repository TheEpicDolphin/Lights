using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strafe : UtilityDecision
{
    Player player;
    Enemy me;
    float maxStrafeDistance = 3.0f;

    public Strafe(string name) : base(name)
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

        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
        {
            //If player does not have firearm, do not strafe
            return 0.0f;
        }

        float strafeSpace;
        Vector2 myPos = me.transform.position;
        RaycastHit2D hit = Physics2D.CircleCast(myPos, me.radius, , 3.0f);
        if(hit)
        {
            strafeSpace = hit.centroid - myPos;
        }

        //Desire to hide based on how long the enemy has been exposed in the player's FOV
        float exposure = Mathf.Min(me.DangerExposureTime() / maxExposureTime, 1);

        float U = Mathf.Min(strafeSpace / maxStrafeDistance, 1);
        return U;
    }

    public override UtilityAction Execute(Dictionary<string, object> memory, Dictionary<string, object> calculated)
    {
        return new Move();
    }
}
