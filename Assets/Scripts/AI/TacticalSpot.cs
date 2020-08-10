using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GeometryUtils;

public class TacticalSpot : MonoBehaviour
{
    Enemy enemy;
    private float weight = 1.0f;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    public float PathDistance(Vector2[] path)
    {
        float d = 0.0f;
        for(int i = 0; i < path.Length - 1; i++)
        {
            d += Vector2.Distance(path[i], path[i + 1]);
        }
        return d;
    }

    /*
     * path: contains list of Vector2 positions from enemy location to tactical spot.
     */
    public float Score(Vector2[] path)
    {
        weight = ValidLocationConsideration() *
                VelocityChangeConsideration(path) *
                OccupiedConsideration() *
                //ExposureConsideration() *
                PlayerPositionConsideration() *
                DistanceConsideration(path) *
                IdlenessConsideration();
        return weight;
    }

    public float ValidLocationConsideration()
    {
        return enemy.navMesh.IsLocationValid(transform.position) ? 1.0f : 0.0f;
    }

    public float VelocityChangeConsideration(Vector2[] path)
    {
        Vector2 curVelocity = enemy.GetVelocity();
        Vector2 theoreticalVelocity = enemy.VelocityToReachPosition(path.Skip(1).First());
        float d = Vector2.Distance(curVelocity, theoreticalVelocity);
        //Debug.Log(d);
        float weight = 1.0f - Mathf.Min(0.6f, d / (2 * enemy.maxSpeed));
        return weight;
    }

    public float OccupiedConsideration()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 0.75f * enemy.radius, 1 << 11);
        if (col != null && col.GetComponent<Enemy>() != enemy)
        {
            return 0.0f;
        }
        else
        {
            return 1.0f;
        }
    }

    public float ExposureConsideration()
    {
        Player player = enemy.player;
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            float tacticalSpotExposure = player.FOVContains(transform.position) ? 1.0f : 0.0f;
            float enemyExposure = enemy.Exposure();

            if(Mathf.Abs(enemyExposure - 0.5f) > 0.25f)
            {
                return Mathf.Abs(enemyExposure - tacticalSpotExposure);
            }
            else
            {
                return 1.0f;
            }
        }
        return 0.0f;
    }

    public float PlayerPositionConsideration()
    {
        Player player = enemy.player;

        Vector2 playerDir = player.transform.position - enemy.transform.position;
        Vector2 midPoint = (player.transform.position + enemy.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);
        /* Check if landmark is closer to AI than to player */
        float c = sepBoundary.SignedDistanceToPoint(transform.position);

        /* TODO: Take into account AI's weapon range */

        float score = c > 0.0f ? 1.0f : 0.0f;
        return score;
    }

    public float DistanceConsideration(Vector2[] path)
    {
        //Takes into account the distance from AI to tactical spot
        float dist = PathDistance(path);
        float score = 1.0f - Mathf.Clamp(dist / (1.5f * enemy.maxTacticalPositionRange), 0.0f, 0.5f);
        return score;
    }

    public float IdlenessConsideration()
    {
        float enemyIdlness = enemy.Idleness();
        if (enemy.BubbleContainsPoint(transform.position))
        {
            return 1.0f - enemyIdlness;
        }
        else
        {
            return 1.0f;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, weight);
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }

}
