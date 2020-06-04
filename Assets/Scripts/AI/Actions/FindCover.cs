using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindCover : UtilityAction
{
    Player player;
    Enemy me;
    

    public FindCover(string name) : base(name)
    {
        
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
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
        {
            return false;
        }

        return true;
    }

    public override float Score(Dictionary<string, object> calculated)
    {
        //Desire is measured as value in range [0, 1], where 0 is low desire and 1 is high desire.

        //TODO: Desire to hide based on ammo remaining


        //Desire to hide based on proximity to target
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        float equippedFirearmRange = firearm.GetRange();
        float dist = Vector3.Distance(player.transform.position, me.transform.position);
        float proximity = 1 - Mathf.Max(equippedFirearmRange - dist, 0.0f) / equippedFirearmRange;


        float U = ;
        return U;
    }

    public override void Run(Dictionary<string, object> calculated)
    {
        Collider2D[] coverColliders = Physics2D.OverlapCircleAll();
        foreach(Collider2D coverCollider in coverColliders)
        {
            Cover cover = coverCollider.gameObject.GetComponent<Cover>();
            if (cover)
            {

            }
        }
    }
}
