using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Consider making this UtilityActionGroup with multiple directions for strafing
public class Strafe : UtilityAction
{
    Enemy me;
    float maxStrafeDistance = 3.0f;

    public Strafe(Enemy me)
    {
        this.me = me;

        considerations = new List<UtilityConsideration>()
        {
            //new ExposureConsideration(me),
        };

        coActions = new HashSet<System.Type>()
        {
            typeof(AimAtPlayer),
            typeof(ShootAtPlayer)
        };
    }

    public override void Execute()
    {
        Player player = me.player;
        bool right = Random.Range(0.0f, 1.0f) > 0.5f;
        Vector2 playerDir = (player.transform.position - me.transform.position).normalized;
        Vector2 strafeDir;
        if (right)
        {
            strafeDir = Vector2.Perpendicular(playerDir);
        }
        else
        {
            strafeDir = -Vector2.Perpendicular(playerDir);
        }
        strafeDir.Normalize();
        Vector2 myPos = me.transform.position;
        RaycastHit2D hit = Physics2D.CircleCast(myPos, me.radius, strafeDir, maxStrafeDistance, (1 << 12));
        Vector2 strafeTargetPos = maxStrafeDistance * strafeDir + myPos;
        if (hit)
        {
            strafeTargetPos = hit.centroid;
        }

        me.AddKnockback(10.0f, strafeDir);
    }
}
