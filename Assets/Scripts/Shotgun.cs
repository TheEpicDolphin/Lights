using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour, IItem, IFirearm
{
    Animator anim;
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
            //Instantiate shotgun blast particle system
            GameObject shotgunBlast = (GameObject) Instantiate(Resources.Load("Prefabs/ShotgunBlast"));
            shotgunBlast.transform.position = barrelExit.position;
            shotgunBlast.transform.rotation = barrelExit.rotation;
            ParticleSystem shotgunBlastPS = shotgunBlast.GetComponent<ParticleSystem>();
            shotgunBlastPS.Play();
            Destroy(shotgunBlast, shotgunBlastPS.main.duration);

            lastT = t;

            //TODO: fix to work with enemies
            IHitable shooter = transform.parent.GetComponentInParent<IHitable>();
            shooter.AddKnockback(knockbackStrength, -dir);

            Vector2 start = barrelExit.position;
            float angle = shotgunBlastPS.shape.angle;
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
                    //TODO: fix to work with player
                    IHitable hitByPellet = hit.collider.GetComponent<IHitable>();
                    if(hitByPellet != null && hitByPellet != shooter)
                    {
                        hitByPellet.AddKnockback((knockbackStrength / numPellets) * (1 - hit.distance / range),
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
