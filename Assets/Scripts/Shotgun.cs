using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour, IItem, IFirearm
{
    Animator anim;
    ParticleSystem blast;
    Transform barrelExit;
    float firerate = 0.8f;
    float range = 20.0f;
    int numPellets = 5;
    float knockbackStrength = 25.0f;
    float lastT = 0.0f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        blast = GetComponentInChildren<ParticleSystem>();
        barrelExit = transform.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Animate(float angle)
    {
        anim.SetFloat("GunAngle", angle);
    }

    public void Shoot()
    {
        Vector2 dir = transform.up;
        float t = Time.time;
        if(t - lastT > 1 / firerate)
        {
            blast.Play();
            lastT = t;

            Player player = transform.parent.GetComponentInParent<Player>();
            player.AddKnockback(knockbackStrength, -dir);

            Vector2 start = barrelExit.position;
            float angle = blast.shape.angle;
            //Do multiple raycasts to hit targets
            for (int i = 0; i < numPellets; i++)
            {
                float randAngle = Random.Range(-angle, angle);
                //Add random noise to pellet direction
                Vector2 pelletDir = Quaternion.AngleAxis(randAngle, Vector3.forward) * dir;
                RaycastHit2D[] hits = Physics2D.RaycastAll(start, pelletDir, range, (1 << 11));
                //Debug.DrawRay(start, range * pelletDir, Color.red, 5.0f);
                
                foreach (RaycastHit2D hit in hits)
                {
                    Enemy enemy = hit.collider.GetComponent<Enemy>();
                    if(enemy != null)
                    {
                        enemy.AddKnockback((knockbackStrength / numPellets) * (1 - hit.distance / range),
                                           pelletDir);
                    }
                }
                
            }
            
        }
        
    }

    public bool ReadyToFire()
    {
        return (Time.time - lastT) > (1 / firerate);
    }

    public float GetRange()
    {
        return range;
    }

    public float GetFireRate()
    {
        return firerate;
    }

    public Transform GetBarrelExit()
    {
        return barrelExit;
    }
}
