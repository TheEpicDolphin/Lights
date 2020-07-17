using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strafe : UtilityAction
{
    Enemy me;
    float maxStrafeDistance = 3.0f;

    public Strafe()
    {
        me = GetComponent<Enemy>();
        Debug.Assert(me != null, "Fail");

        considerations = new List<UtilityConsideration>()
        {
            new ExposureConsideration(me, UtilityRank.Medium),
            new IdlenessConsideration(me, UtilityRank.Medium),
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
        Vector2 strafeTarget = maxStrafeDistance * strafeDir;
        if (hit)
        {
            strafeTarget = hit.centroid;
        }
        me.NavigateTo(strafeTarget);
    }
}
