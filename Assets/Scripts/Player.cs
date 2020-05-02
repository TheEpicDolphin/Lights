using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    public float radius;

    float playerSpeed = 8.0f;

    float moveHorizontal;
    float moveVertical;
    Vector2 relMousePos;

    Animator animator;
    Animator handAnimator;
    Rigidbody2D rb;
    Hand hand;

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

        //inventory.handgunAmmo = 200;


    }

    // Update is called once per frame
    void Update()
    {

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        relMousePos = Input.mousePosition - Camera.main.WorldToScreenPoint(hand.transform.position);
        relMousePos.Normalize();

        Vector2 vDesired = Vector2.zero;
        Vector2 movement = moveVertical * new Vector2(0, 1) + moveHorizontal * new Vector2(1, 0);
        if (movement.magnitude > 1.0f)
        {
            movement = movement.normalized;
        }
        //Animate player facing direction
        animator.SetFloat("movement", movement.magnitude);
        animator.SetFloat("facingY", relMousePos.y);
        animator.SetFloat("facingX", relMousePos.x);

        if (movement.magnitude > 0.25f)
        {
            //Only animate if beyond a certain threshold
            movement.Normalize();
            animator.SetFloat("dy", movement.y);
            animator.SetFloat("dx", movement.x);
            vDesired = movement.normalized * playerSpeed;
        }

        hand.relMousePos = relMousePos;

        Vector2 a = (vDesired - rb.velocity) * 20.0f;
        //Prevent unrealistic force
        //a = Mathf.Clamp(a.magnitude, 0.0f, 7500.0f) * a.normalized;
        rb.AddForce(a, ForceMode2D.Force);
    }

    private void LateUpdate()
    {
        
    }

    public void AddKnockback(float strength, Vector2 dir)
    {
        rb.AddForce(strength * dir, ForceMode2D.Impulse);
    }

}

