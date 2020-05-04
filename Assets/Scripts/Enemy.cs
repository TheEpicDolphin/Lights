using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, INavAgent
{
    public NavigationMesh navMesh;
    public Player player;
    Rigidbody2D rb;
    float radius = 1.0f;
    float enemySpeed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Navigate(player.transform.position);
    }

    public void Navigate(Vector2 destination)
    {
        Vector2[] shortestPath = navMesh.GetShortestPathFromTo(transform.position, destination);

        Vector2 nextPoint = shortestPath[0];
        Vector2 curPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 dir = (nextPoint - curPos).normalized;
        Vector2 offset = Vector2.Perpendicular(dir) * radius;
        RaycastHit2D hit;

        Debug.DrawRay(curPos + offset, 3.0f * dir, Color.red);
        Debug.DrawRay(curPos - offset, 3.0f * dir, Color.red);
        if (hit = Physics2D.Raycast(curPos + offset, dir, 3.0f, (1 << 12)))
        {
            curPos = hit.point + radius * hit.normal;
        }
        else if (hit = Physics2D.Raycast(curPos - offset, dir, 3.0f, (1 << 12)))
        {
            curPos = hit.point + radius * hit.normal;
        }
        
        Vector2 vDesired = (nextPoint - curPos).normalized * enemySpeed;

        float k = (1 / Time.deltaTime) * 0.4f;
        Vector2 f = k * (vDesired - rb.velocity);
        //Prevent unrealistic forces by clamping to range
        f = Mathf.Clamp(f.magnitude, 0, 250.0f) * f.normalized;
        rb.AddForce(f, ForceMode2D.Force);

    }

}
