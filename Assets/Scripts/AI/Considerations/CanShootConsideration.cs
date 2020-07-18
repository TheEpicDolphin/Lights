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

    public override float Score()
    {

        Vector2 target = me.GetShootingTarget();
        //Check if AI has gun equipped
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            //Check if there is anything blocking line of sight from AI to player
            //and if gun is ready to fire another round
            RaycastHit2D hit = Physics2D.Linecast(me.transform.position, target);
            if (hit.collider.GetComponent<Player>() == null && firearm.ReadyToFire())
            {
                return 1.0f;
            }
        }

        return 0.0f;

    }
}
