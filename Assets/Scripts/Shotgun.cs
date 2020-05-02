using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour, IItem, IFirearm
{
    Animator anim;
    ParticleSystem blast;
    float firerate = 0.8f;
    float lastT = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        blast = GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Animate(float angle)
    {
        anim.SetFloat("GunAngle", angle);
    }

    public void Shoot(Vector2 target)
    {
        float t = Time.time;
        if(t - lastT > 1 / firerate)
        {
            blast.Play();
            Debug.Log("Shoot");
            lastT = t;

            Player player = transform.parent.GetComponentInParent<Player>();
            player.AddKnockback(25.0f, -transform.up);
        }
        
    }
}
