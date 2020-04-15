using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDetector : MonoBehaviour
{
    Beam beam;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 bottomLeftOrigin = -transform.parent.right - transform.parent.up;
        Vector3 topLeftOrigin = -transform.parent.right + transform.parent.up;
        Vector3 topRightOrigin = transform.parent.right + transform.parent.up;
        Vector3 bottomRightOrigin = transform.parent.right - transform.parent.up;

        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        boxCollider.size = new Vector3((topRightOrigin - topLeftOrigin).magnitude, (topLeftOrigin - bottomLeftOrigin).magnitude, 100);
        boxCollider.center = new Vector3(0, 0, 50);
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


/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDetector : MonoBehaviour
{
    List<Obstacle> obstacles;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 bottomLeftOrigin = -transform.parent.right - transform.parent.up;
        Vector3 topLeftOrigin = -transform.parent.right + transform.parent.up;
        Vector3 topRightOrigin = transform.parent.right + transform.parent.up;
        Vector3 bottomRightOrigin = transform.parent.right - transform.parent.up;

        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        boxCollider.size = new Vector3((topRightOrigin - topLeftOrigin).magnitude, (topLeftOrigin - bottomLeftOrigin).magnitude, 100);
        boxCollider.center = new Vector3(0, 0, 50);
    }

    private void FixedUpdate()
    {
        obstacles = new List<Obstacle>();
    }

    private void OnTriggerStay(Collider other)
    {
        Obstacle obstacle = other.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            obstacles.Add(obstacle);
        }
    }

}
*/
