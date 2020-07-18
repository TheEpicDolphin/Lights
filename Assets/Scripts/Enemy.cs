using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathUtils;
using AlgorithmUtils;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

//[XmlRoot("EnemyAI")]

//[RequireComponent(typeof(AimAtPlayer))]
//[RequireComponent(typeof(ShootAtPlayer))]
//[RequireComponent(typeof(NavigateToStaticDestination))]
//[RequireComponent(typeof(TakeCover))]
//[RequireComponent(typeof(ExposeFromCover))]
//[RequireComponent(typeof(Strafe))]
//[RequireComponent(typeof(IdleNavigation))]
public class Enemy : MonoBehaviour, INavAgent, IHitable
{
    public NavigationMesh navMesh;
    public Player player;
    Rigidbody2D rb;
    public float speed = 5.0f;
    public Hand hand;
    public float radius;

    float exposedStartTime;
    float hiddenStartTime;
    float idleStartTime;
    INavTarget navTarget;
    Landmark claimedCover = null;

    Vector2 vDesired = Vector2.zero;

    UtilityAI utilAI;

    //[XmlElement("nav", typeof(NavigateToStaticDestination))]
    //[XmlElement("aim", typeof(AimAtPlayer))]
    //[XmlElement("shoot", typeof(ShootAtPlayer))]
    //[XmlElement("expose_from_cover", typeof(ExposeFromCover))]
    //[XmlElement("take_cover", typeof(TakeCover))]
    //[XmlElement("strafe", typeof(Strafe))]
    public UtilityAction[] utilityActions;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<CircleCollider2D>().radius = navMesh.aiRadius;
        this.radius = GetComponent<CircleCollider2D>().radius;

        idleStartTime = Time.time;
        exposedStartTime = Time.time;
        hiddenStartTime = Time.time;

        hand = GetComponentInChildren<Hand>();
        
        GameObject firearm = (GameObject)Instantiate(Resources.Load("Prefabs/Shotgun"));
        hand.EquipObject(firearm);

        navTarget = new StrafeTarget(transform.position);

        gameObject.AddComponent<AimAtPlayer>();
        gameObject.AddComponent<ShootAtPlayer>();
        gameObject.AddComponent<NavigateToStaticDestination>();
        gameObject.AddComponent<TakeCover>();
        gameObject.AddComponent<ExposeFromCover>();
        gameObject.AddComponent<Strafe>();
        gameObject.AddComponent<IdleNavigation>();

        utilityActions = GetComponents<UtilityAction>();
        utilAI = new UtilityAI(utilityActions);

    }

    public static UtilityAI CreateFromXML()
    {
        UtilityAI uai = XMLOp.Deserialize<UtilityAI>(
            Path.Combine(Application.dataPath, "XML", "ai.xml"));
        Debug.Log(Path.Combine(Application.dataPath, "XML", "ai.xml"));
        Debug.Log(uai.actions.Length);
        return uai;
    }

    void Update()
    {
        vDesired = Vector2.zero;
        Sense();
        hand.Animate();
        utilAI.RunOptimalActions();
        //NavigateTo(player.transform.position);
        DampMovement();
    }

    public void NavigateTo(Vector2 destination)
    {
        Vector2 curPos = new Vector2(transform.position.x, transform.position.y);
        if(Vector2.Distance(curPos, destination) <= 1e-2f)
        {
            //If destination is current position, do nothing
            return;
        }

        Vector2[] shortestPath = navMesh.GetShortestPathFromTo(curPos, destination);
        Vector2 nextPoint = shortestPath[0];
        vDesired = (nextPoint - curPos).normalized * speed;        
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
        if(rb.velocity.sqrMagnitude > 1e-5f)
        {
            idleStartTime = Time.time;
        }

        bool visibleToPlayer = player.FOVContains(transform.position);
        if (!visibleToPlayer)
        {
            exposedStartTime = Time.time;
        }
        else
        {
            hiddenStartTime = Time.time;
        }
    }    

    public float ExposedTime()
    {
        return Time.time - exposedStartTime;
    }

    public float HiddenTime()
    {
        return Time.time - hiddenStartTime;
    }

    public float IdleTime()
    {
        return Time.time - idleStartTime;
    }

    public Vector2 GetShootingTarget()
    {
        return player.transform.position;
    }

    public void SetNavTarget(INavTarget newNavTarget)
    {
        navTarget = newNavTarget;
    }

    public INavTarget GetNavTarget()
    {
        return navTarget;
    }

}
