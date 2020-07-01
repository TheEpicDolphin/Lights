using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathUtils;

public class Enemy : MonoBehaviour, INavAgent, IHitable
{
    public NavigationMesh navMesh;
    public Player player;
    Rigidbody2D rb;
    public float speed = 2.0f;
    public Hand hand;
    public float radius;

    //(less exposed) 0 --> 1 (more exposed)
    public float exposure = 0.0f;
    WeightedMovingAverage wma = new WeightedMovingAverage(10);

    Vector2 vDesired = Vector2.zero;

    UtilityAI utilAI;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<CircleCollider2D>().radius = navMesh.aiRadius;
        this.radius = GetComponent<CircleCollider2D>().radius;

        hand = GetComponentInChildren<Hand>();
        
        GameObject firearm = (GameObject)Instantiate(Resources.Load("Prefabs/Shotgun"));
        hand.EquipObject(firearm);

        utilAI = new UtilityAI(new List<UtilityBucket>()
        {
            new InCombatBucket("combat"),
            new InCoverBucket("cover")
        });
        utilAI.AddMemory("player", player);
        utilAI.AddMemory("me", this);
    }

    void Update()
    {
        vDesired = Vector2.zero;
        Sense();
        hand.Animate();
        utilAI.RunOptimalAction();
        //NavigateTo(player.transform.position);

        DampMovement();
    }

    public void NavigateTo(Vector2 destination)
    {
        Vector2[] shortestPath = navMesh.GetShortestPathFromTo(transform.position, destination);

        Vector2 nextPoint = shortestPath[0];
        Vector2 curPos = new Vector2(transform.position.x, transform.position.y);
        vDesired = (nextPoint - curPos).normalized * speed;        
    }

    public void MoveTo(Vector2 destination)
    {
        Vector2 curPos = new Vector2(transform.position.x, transform.position.y);
        vDesired = (destination - curPos).normalized * speed;
    }

    public void NavigateToWhileAvoiding(Vector2 destination, Vector2 avoid)
    {
        Vector2[] shortestPath = navMesh.GetShortestPathFromTo(transform.position, destination,
        (e) =>
        {
            Triangle tri = (Triangle)e.GetNode();
            float dist = Vector2.Distance(tri.Centroid(), avoid);
            return 1.0f + Mathf.Max(5.0f - dist, 0.0f);
        });

        Vector2 nextPoint = shortestPath[0];
        Vector2 curPos = new Vector2(transform.position.x, transform.position.y);
        vDesired = (nextPoint - curPos).normalized * speed;
    }

    private void DampMovement()
    {
        float k = (1 / Time.deltaTime) * 0.4f;
        Vector2 f = k * (vDesired - rb.velocity);
        //Prevent unrealistic forces by clamping to range
        f = Mathf.Clamp(f.magnitude, 0, 250.0f) * f.normalized;
        rb.AddForce(f, ForceMode2D.Force);
    }

    public void AddKnockback(float strength, Vector2 dir)
    {
        //rb.AddForce(strength * dir, ForceMode2D.Impulse);
    }

    public void Attack()
    {
        hand.Attack();
    }

    public void Sense()
    {
        bool visibleToPlayer = player.FOVContains(transform.position);
        this.exposure = wma.Update(visibleToPlayer ? 1.0f : 0.0f);
    }    
    
}
