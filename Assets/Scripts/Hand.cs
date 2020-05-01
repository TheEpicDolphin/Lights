using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public Vector2 relMousePos = Vector2.zero;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("facingY", relMousePos.y);
        animator.SetFloat("facingX", relMousePos.x);

        if (GetComponentInChildren<Shotgun>())
        {
            Shotgun shotgun = GetComponentInChildren<Shotgun>();
            Animator shotgunAnim = shotgun.GetComponentInChildren<Animator>();

            float angle = Vector2.SignedAngle(Vector2.right, relMousePos);
            if(angle < 0.0f)
            {
                angle = 360.0f + angle;
            }
            shotgunAnim.SetFloat("GunAngle", angle);
        }
    }

    private void LateUpdate()
    {
        Vector3 relHandDir = new Vector3(relMousePos.x, relMousePos.y, 0.0f);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, relHandDir);
    }
}
