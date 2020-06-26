using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour, IHitable
{
    public float radius;
    public float FOVAngle = 120.0f;
    public float FOVRadius = 20.0f;

    float playerSpeed = 8.0f;

    float moveHorizontal;
    float moveVertical;

    Animator animator;
    Animator handAnimator;
    Rigidbody2D rb;
    Vector2 lastVelocity = Vector2.zero;
    Vector2 eLast = Vector2.zero;
    public Hand hand;
    public VisibilityPolygon visibilityPolygon;

    //public Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject sprite = transform.Find("avatar").gameObject;
        animator = sprite.GetComponent<Animator>();

        transform.forward = -Camera.main.transform.forward;
        transform.up = Camera.main.transform.up;

        hand = GetComponentInChildren<Hand>();

        visibilityPolygon = GetComponentInChildren<VisibilityPolygon>();
        //inventory.handgunAmmo = 200;
        radius = GetComponent<CircleCollider2D>().radius;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            hand.Attack();
        }

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 aimTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 relMousePos = aimTarget - hand.transform.position;
        Vector2 relHandDir = relMousePos.normalized;

        Vector2 vDesired = Vector2.zero;
        Vector2 movement = moveVertical * new Vector2(0, 1) + moveHorizontal * new Vector2(1, 0);
        if (movement.magnitude > 1.0f)
        {
            movement = movement.normalized;
        }
        //Animate player facing direction
        animator.SetFloat("movement", movement.magnitude);
        animator.SetFloat("facingY", relHandDir.y);
        animator.SetFloat("facingX", relHandDir.x);

        if (movement.magnitude > 0.25f)
        {
            //Only animate if beyond a certain threshold
            movement.Normalize();
            animator.SetFloat("dy", movement.y);
            animator.SetFloat("dx", movement.x);
            vDesired = movement * playerSpeed;
        }

        
        hand.AimWeaponAtTarget(aimTarget);
        hand.Animate();
        //visibilityPolygon.Draw();
        visibilityPolygon.DrawSlice(LookDir(), FOVAngle, FOVRadius);

        float k = (1 / Time.deltaTime) * 0.4f;
        Vector2 f = k * (vDesired - rb.velocity);
        //Prevent unrealistic forces by clamping to range
        f = Mathf.Clamp(f.magnitude, 0, 250.0f) * f.normalized;
        rb.AddForce(f, ForceMode2D.Force);

    }

    private void LateUpdate()
    {
        
    }

    public void AddKnockback(float strength, Vector2 dir)
    {
        rb.AddForce(strength * dir, ForceMode2D.Impulse);
    }

    public bool FOVContains(Vector2 p)
    {
        return visibilityPolygon.SliceContainsPoint(p, LookDir(), FOVAngle, FOVRadius);
    }

    public bool IsVisibleFrom(Vector2 p)
    {
        return visibilityPolygon.OutlineContainsPoint(p);
    }

    public Vector2 LookDir()
    {
        Vector2 playerPos = transform.position;
        return (hand.AimTarget() - playerPos).normalized;
    }
}

