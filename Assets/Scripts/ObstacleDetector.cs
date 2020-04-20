using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Consider using boxcast instead
public class ObstacleDetector : MonoBehaviour
{
    Beam beam;
    // Start is called before the first frame update
    void Start()
    {
        beam = GetComponentInParent<Beam>();
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.offset = new Vector2((beam.lims[1].x + beam.lims[0].x) / 2.0f, beam.beamLength / 2.0f);
        col.size = new Vector2(beam.lims[1].x - beam.lims[0].x, beam.beamLength);
    }

    private void FixedUpdate()
    {

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Obstacle obstacle = collision.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            if (!beam.obstacles.Contains(obstacle))
            {
                beam.obstacles.Add(obstacle);
            }
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Obstacle obstacle = collision.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            if (beam.obstacles.Contains(obstacle))
            {
                beam.obstacles.Remove(obstacle);
            }
        }
    }
}

