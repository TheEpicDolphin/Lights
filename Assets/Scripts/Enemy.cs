using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, INavAgent, IHitable
{
    public NavigationMesh navMesh;
    public Player player;
    Rigidbody2D rb;
    public float speed = 2.0f;
    public Hand hand;
    float exposureStartTime;
    float hidingStartTime;
    public float radius;
    

    Vector2 vDesired = Vector2.zero;

    //UtilityAI uai;
    UtilityAction action;
    UtilityBucket combatBucket;
    Dictionary<string, object> memory;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<CircleCollider2D>().radius = navMesh.aiRadius;
        this.radius = GetComponent<CircleCollider2D>().radius;

        exposureStartTime = Time.time;
        hidingStartTime = Time.time;
        

        hand = GetComponentInChildren<Hand>();
        
        GameObject firearm = (GameObject)Instantiate(Resources.Load("Prefabs/Shotgun"));
        hand.EquipObject(firearm);

        action = new AimAtDynamicTarget(this, player.transform);
        memory = new Dictionary<string, object>();
        memory["player"] = player;
        memory["me"] = this;
        CreateCombatUtilityBucket();
    }

    void Update()
    {
        vDesired = Vector2.zero;
        Sense();
        hand.Animate();
        combatBucket.RunOptimalAction(memory);
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
        if (!visibleToPlayer)
        {
            exposureStartTime = Time.time;
        }
        else
        {
            hidingStartTime = Time.time;
        }
    }    

    public float DangerExposureTime()
    {
        return Time.time - exposureStartTime;
    }

    public float HiddenTime()
    {
        return Time.time - hidingStartTime;
    }
    
    public void CreateCombatUtilityBucket()
    {
        combatBucket = new UtilityBucket(
            "Combat Bucket",
            new List<UtilityDecision>()
            {
                new ShootAtPlayer("shoot"),
                new AimAtPlayer("aim"),
                new TakeCover("take cover"),
                new ExposeFromCover("expose"),
                new Strafe("strafe")
            });
    }
}
