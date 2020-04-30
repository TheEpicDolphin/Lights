using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    public float radius;
    public Vector3 velocity;

    float playerSpeed = 8.0f;

    //public Firearm firearm;
    //Item equippedItem;

    float moveHorizontal;
    float moveVertical;
    Vector2 relMousePos;

    Animator animator;
    Rigidbody rb;

    //public Inventory inventory;

    Transform hand;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        GameObject sprite = transform.Find("avatar").gameObject;
        animator = sprite.GetComponent<Animator>();

        transform.forward = -Camera.main.transform.forward;
        transform.up = Camera.main.transform.up;

        hand = transform.Find("hand");

        //inventory.handgunAmmo = 200;


    }

    // Update is called once per frame
    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        relMousePos = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        relMousePos.Normalize();
        Vector3 relHandDir = new Vector3(relMousePos.x, relMousePos.y, 0.0f);
        hand.position = transform.position + 1.0f * relHandDir;
        hand.rotation = Quaternion.LookRotation(relHandDir, Vector3.up);

        //hand.RotateAround(transform.position, Vector3.forward, ?);

        if (Input.GetMouseButton(0))
        {
            //firearm.Shoot(animator, ref inventory);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] overlapped = Physics2D.OverlapCircleAll(transform.position, 1.0f);
            foreach(Collider2D overlap in overlapped)
            {
                
            }
            //Attach item to hand
            
            //equippedItem.transform.parent = hand;
            //equippedItem.transform.localPosition = Vector3.zero;
            //equippedItem.transform.localRotation = Quaternion.identity;
        }

    }

    private void FixedUpdate()
    {
        velocity = Vector3.zero;
        Vector3 movement = moveVertical * new Vector3(0.0f, 1.0f, 0) + moveHorizontal * new Vector3(1.0f, 0.0f, 0.0f);
        if (movement.magnitude > 1.0f)
        {
            movement = movement.normalized;
        }
        animator.SetFloat("movement", movement.magnitude);
        animator.SetFloat("facingY", relMousePos.y);
        animator.SetFloat("facingX", relMousePos.x);

        if (movement.magnitude > 0.25f)
        {
            movement.Normalize();
            animator.SetFloat("dy", movement.y);
            animator.SetFloat("dx", movement.x);
            velocity = movement.normalized * playerSpeed;
            transform.Translate(movement.normalized * playerSpeed * Time.fixedDeltaTime, Space.World);
        }

    }

    private void OnTriggerStay(Collider other)
    {
        
    }

}

