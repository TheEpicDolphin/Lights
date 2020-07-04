using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanShootConsideration : UtilityConsideration
{
    public CanShootConsideration(int rank) : base(rank)
    {

    }

    public override Vector2 Score(Dictionary<string, object> memory)
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
                RaycastHit2D hit = Physics2D.Linecast(me.transform.position, target);
                if (hit.collider.GetComponent<Player>() != null)
                {
                    return Vector2.zero;
                }

                //Check if gun is ready to fire another round
                if (!firearm.ReadyToFire())
                {
                    return Vector2.zero;
                }

                return new Vector2(1.0f, 1.0f);
            }
        }

        return Vector2.zero;

    }
}
