using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    Vector2 direction = Vector2.zero;
    Animator animator;
    GameObject equippedObject;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("facingY", direction.y);
        animator.SetFloat("facingX", direction.x);

        IItem item = equippedObject?.GetComponent<IItem>();
        if (item != null)
        {
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            if (angle < 0.0f)
            {
                angle = 360.0f + angle;
            }
            item.Animate(angle);
        }
    }

    public void Attack()
    {
        IFirearm firearm = equippedObject?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            firearm.Shoot(direction);
        }
    }


    private void LateUpdate()
    {
        Vector3 relHandDir = new Vector3(direction.x, direction.y, 0.0f);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, relHandDir);
    }

    public void EquipObject(GameObject equippedObject)
    {
        this.equippedObject = equippedObject;
        this.equippedObject.transform.parent = transform;
        this.equippedObject.transform.localPosition = Vector3.zero;
        this.equippedObject.transform.localRotation = Quaternion.identity;
        this.equippedObject.layer = 16;
    }

    public void UnequipObject()
    {
        this.equippedObject.transform.parent = null;
        this.equippedObject.layer = 15;
        this.equippedObject = null;
    }

    public bool HasObjectEquipped()
    {
        return equippedObject != null;
    }

    public GameObject GetEquippedObject()
    {
        return equippedObject;
    }

    public void SetHandDirection(Vector2 handDir)
    {
        direction = handDir;
    } 

    public Vector2 GetHandDirection()
    {
        return direction;
    }
}
