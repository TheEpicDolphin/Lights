using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanShootConsideration : UtilityConsideration
{
    Enemy me;
    public CanShootConsideration(Enemy me, UtilityRank rank) : base(rank)
    {
        this.me = me;
    }

    public override bool Score(out float weight)
    {
        if (memory.ContainsKey("shooting_target") && memory.ContainsKey("me"))
        {
            Vector2 target = (Vector2)memory["shooting_target"];
            Enemy me = (Enemy)memory["me"];
            //Check if AI has gun equipped
            IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
            if (firearm != null)
            {
                //Check if there is anything blocking line of sight from AI to player
                //and if gun is ready to fire another round
                RaycastHit2D hit = Physics2D.Linecast(me.transform.position, target);
                if (hit.collider.GetComponent<Player>() == null && !firearm.ReadyToFire())
                {
                    weight = 1.0f;
                    return true;
                }
            }
        }

        weight = 0.0f;
        return false;

    }
}
