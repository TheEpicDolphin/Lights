using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class Hand : MonoBehaviour
{
    Animator animator;
    GameObject equippedObject;
    Vector3 aimTarget;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        aimTarget = Vector2.zero;
    }

    public void Animate()
    {
        Vector2 direction = transform.up;
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
            firearm.Shoot();
        }
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

    public void AimWeaponAtTarget(Vector3 target)
    {
        IFirearm firearm = GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            this.aimTarget = target;
            Transform barrelExit = firearm.GetBarrelExit();
            Vector2 aimTarget2D = aimTarget;

            Vector2 n = Vector2.Perpendicular(barrelExit.up);
            Plane2D plane = new Plane2D(n, barrelExit.position);
            float r = plane.DistanceToPoint(transform.position);
            Vector2 tangent;
            if (Geometry.CircleTangent(transform.position, r, aimTarget, -Vector3.forward, out tangent))
            {
                Vector2 aimDir = aimTarget2D - tangent;
                transform.rotation = Quaternion.LookRotation(Vector3.forward, aimDir);
            }
        }
    }

    public Vector2 AimTarget()
    {
        return aimTarget;
    }
}
