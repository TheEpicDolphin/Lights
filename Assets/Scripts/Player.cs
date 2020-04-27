using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    public float radius;
    public Vector3 velocity;

    float playerSpeed = 15.0f;

    //public Firearm firearm;
    //Item equippedItem;

    float moveHorizontal;
    float moveVertical;
    Vector3 relMousePos;

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

        if (Input.GetMouseButton(1))
        {
            //hand.GetChild().GetComponent<MeshRenderer>().enabled = Input.GetMouseButton(1);
            hand.RotateAround(new Vector3(transform.position.x, 1.0f, transform.position.z), hand.right, -2.0f * Input.GetAxis("Mouse Y"));
        }
        else
        {
            relMousePos = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            relMousePos = new Vector3(relMousePos.x, relMousePos.y, 0.0f).normalized;

            //Move hand around body
            Vector3 relHandDir = new Vector3(relMousePos.x, 0.0f, relMousePos.y);
            hand.position = new Vector3(transform.position.x, 1.0f, transform.position.z) + 2.0f * relHandDir;
            hand.rotation = Quaternion.LookRotation(relHandDir, Vector3.up);
        }

        //Debug.DrawRay(hand.transform.position, hand.transform.up, Color.red);

        if (Input.GetMouseButton(0))
        {
            //firearm.Shoot(animator, ref inventory);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            //Attach sword to hand
            //hand = transform.Find("Hand");
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
            Vector3.Normalize(movement);
            animator.SetFloat("dy", movement.z);
            animator.SetFloat("dx", movement.x);
            velocity = movement.normalized * playerSpeed;
            transform.Translate(movement.normalized * playerSpeed * Time.fixedDeltaTime, Space.World);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

}

