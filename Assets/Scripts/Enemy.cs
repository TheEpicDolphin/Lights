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

    Vector2 curBubblePosition;
    const float bubbleRadius = 1.0f;
    float idleness = 0.0f;

    float exposure = 0.0f;
    bool wasVisibleLastFrame = false;

    public float maxSpeed = 7.0f;
    private float maxExposure = 5.0f;
    private float maxIdleness = 6.0f;

    public float maxTacticalPositionRange = 10.0f;

    private Vector2 vDesired = Vector2.zero;

    private TacticalSpot[] tacticalSpots;

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

        hand = GetComponentInChildren<Hand>();
        
        GameObject firearm = (GameObject)Instantiate(Resources.Load("Prefabs/Shotgun"));
        hand.EquipObject(firearm);

        utilAI = new UtilityAI(new UtilityAction[]
        {
            new AimAtPlayer(this),
            new ShootAtPlayer(this),
            new TacticalPositioning(this),
        });

        tacticalSpots = GetComponentsInChildren<TacticalSpot>();
        curBubblePosition = transform.position;
    }

    void Update()
    {
        vDesired = Vector2.zero;
        Sense();
        hand.Animate();
        utilAI.RunOptimalActions();
        //NavigateTo(new Vector2(-3, 0));
        DampMovement();
        //Debug.Log(Exposure());
        Debug.Log(Idleness());
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
        Vector2 nextPoint = shortestPath.Skip(1).First();
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
        if(!BubbleContainsPoint(transform.position))
        {
            curBubblePosition = transform.position;
            idleness = 0.0f;
        }
        else
        {
            idleness = Mathf.Min(maxIdleness, idleness + Time.deltaTime);
        }

        bool visibleToPlayer = player.FOVContains(transform.position);
        if (visibleToPlayer)
        {
            if(visibleToPlayer ^ wasVisibleLastFrame)
            {
                exposure = 0.0f;
            }
            exposure = Mathf.Min(maxExposure, exposure + Time.deltaTime);
        }
        else
        {
            if (visibleToPlayer ^ wasVisibleLastFrame)
            {
                exposure = maxExposure;
            }
            exposure = Mathf.Max(0.0f, exposure - Time.deltaTime);
        }
        wasVisibleLastFrame = visibleToPlayer;
    }    

    public bool BubbleContainsPoint(Vector2 pos)
    {
        return Vector2.Distance(pos, curBubblePosition) <= bubbleRadius;
    }

    public float Exposure()
    {
        return this.exposure / maxExposure;
    }

    public float Idleness()
    {
        if(this.idleness > 3.0f)
        {
            return this.idleness / maxIdleness;
        }
        else
        {
            return 0.0f;
        }
        
    }

    public Vector2 GetShootingTarget()
    {
        return player.transform.position;
    }

    /*
    public List<Vector2> GetTacticalSpots()
    {
        float s = maxTacticalPositionRange / Mathf.Sqrt(2);
        List<Vector2> spots = new List<Vector2>();
        Vector2 pos2d = transform.position;

        int Ny = 5;
        int Nx = 5;
        Vector2 center = transform.position;
        for (int i = -(Ny - 1) / 2; i <= (Ny - 1) / 2; i++)
        {
            float y = center.y + i * s / Ny;
            for (int j = -(Nx - 1) / 2; j <= (Nx - 1) / 2; j++)
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
    */

    public TacticalSpot[] GetTacticalSpots()
    {
        Algorithm.Shuffle(ref tacticalSpots);
        return tacticalSpots;
    }

    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }

    

}
