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
    
    public Hand hand;
    public float radius;

    float exposedStartTime;
    float idleStartTime;

    float exposure = 0.0f;

    public float maxSpeed = 7.0f;
    public float maxExposure = 5.0f;

    public float maxTacticalPositionRange = 10.0f;

    private Vector2 vDesired = Vector2.zero;
    UtilityAI utilAI;

    //[XmlElement("nav", typeof(NavigateToStaticDestination))]
    //[XmlElement("aim", typeof(AimAtPlayer))]
    //[XmlElement("shoot", typeof(ShootAtPlayer))]
    //[XmlElement("expose_from_cover", typeof(ExposeFromCover))]
    //[XmlElement("take_cover", typeof(TakeCover))]
    //[XmlElement("strafe", typeof(Strafe))]

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<CircleCollider2D>().radius = navMesh.aiRadius;
        this.radius = GetComponent<CircleCollider2D>().radius;

        idleStartTime = Time.time;
        exposedStartTime = Time.time;

        hand = GetComponentInChildren<Hand>();
        
        GameObject firearm = (GameObject)Instantiate(Resources.Load("Prefabs/Shotgun"));
        hand.EquipObject(firearm);

        utilAI = new UtilityAI(new UtilityAction[]
        {
            new AimAtPlayer(this),
            new ShootAtPlayer(this),
            new TacticalPositioning(this),
        });


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
        //utilAI.RunOptimalActions();
        NavigateTo(new Vector2(-3, 0));
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
        vDesired = VelocityToReachPosition(nextPoint);
    }

    public void MoveTo(Vector2 destination)
    {
        vDesired = VelocityToReachPosition(destination);
    }

    public Vector2 VelocityToReachPosition(Vector2 target)
    {
        Vector2 curPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 dir = (target - curPos).normalized;
        float mag = Mathf.Min(1.0f, Vector2.Distance(curPos, target) / 0.5f) * maxSpeed;
        return mag * dir;
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
        if (visibleToPlayer)
        {
            exposure = Mathf.Min(maxExposure, exposure + Time.deltaTime);
        }
        else
        {
            exposure = Mathf.Max(0.0f, exposure - Time.deltaTime);
            exposedStartTime = Time.time;
        }
    }    

    public float ExposedTime()
    {
        return Time.time - exposedStartTime;
    }

    public float Exposure()
    {
        return this.exposure / maxExposure;
    }

    public float IdleTime()
    {
        return Time.time - idleStartTime;
    }

    public Vector2 GetShootingTarget()
    {
        return player.transform.position;
    }

    public List<Vector2> GetTacticalPositioningCandidates()
    {
        float s = maxTacticalPositionRange / Mathf.Sqrt(2);
        List<Vector2> spots = new List<Vector2>();
        Vector2 pos2d = transform.position;

        int Ny = 5;
        int Nx = 5;
        Vector2 center = transform.position;
        for (int i = -Ny / 2; i <= Ny / 2; i++)
        {
            float y = center.y + i * s / Ny;
            for (int j = -Nx / 2; j <= Nx / 2; j++)
            {
                float x = center.x + j * s / Nx;
                Vector2 p = new Vector2(x, y);
                if (navMesh.IsLocationValid(p))
                {
                    spots.Add(p);
                }
            }
        }
        return spots;
    }

    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }
}
