using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GeometryUtils;

public class TacticalSpot : MonoBehaviour
{
    Enemy enemy;
    private float weight = 0.0f;

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
                ExposureConsideration() *
                TacticConsideration(path);
        return weight;
    }

    public float ValidLocationConsideration()
    {
        return enemy.navMesh.IsLocationValid(transform.position) ? 1.0f : 0.0f;
    }

    public float VelocityChangeConsideration(Vector2[] path)
    {
        Vector2 curPos = enemy.transform.position;
        Vector2 curVelocity = enemy.GetVelocity();
        Vector2 theoreticalVelocity = enemy.VelocityToReachPosition(path.First());
        float d = Vector2.Distance(curVelocity, theoreticalVelocity);
        float weight = 1.0f - Mathf.Max(0.6f, d / (2 * enemy.maxSpeed));
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
        IFirearm firearm = enemy.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            float tacticalSpotExposure = player.FOVContains(transform.position) ? 1.0f : 0.0f;
            //AI seeks a change in exposure
            return Mathf.Abs(enemy.Exposure() - tacticalSpotExposure);
        }
        return 0.0f;
    }

    public float TacticConsideration(Vector2[] path)
    {
        //TODO: Split up below into two different considerations
        Player player = enemy.player;

        Vector2 playerDir = player.transform.position - enemy.transform.position;
        Vector2 midPoint = (player.transform.position + enemy.transform.position) / 2;
        Plane2D sepBoundary = new Plane2D(-playerDir.normalized, midPoint);
        /* Check if landmark is closer to AI than to player */
        float c = sepBoundary.SignedDistanceToPoint(transform.position);

        /* Take into account distance from AI to landmark */
        float dist = PathDistance(path);
        /* higher proximity = tactical spot is closer to AI  */
        float proximity = 1.0f - Mathf.Clamp(dist / (1.5f * enemy.maxTacticalPositionRange), 0.0f, 0.5f);

        /* TODO: Take into account AI's weapon range */

        /* Score landmark */
        float score = Mathf.Max(0, (1 - Mathf.Exp(-10 * c)) * proximity);

        return score;
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, weight);
        Gizmos.DrawSphere(transform.position, 0.25f);
    }

}
