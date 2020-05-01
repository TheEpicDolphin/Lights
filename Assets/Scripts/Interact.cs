using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    Player player;
    List<Beam> beams = new List<Beam>();

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(beams.Count > 0)
            {
                Beam closestBeam = beams[0];
                float closestDist = Vector3.Distance(closestBeam.transform.position, transform.position);
                foreach(Beam beam in beams)
                {
                    float newDist = Vector3.Distance(closestBeam.transform.position, transform.position);
                    if (newDist < closestDist)
                    {
                        closestDist = newDist;
                        closestBeam = beam;
                    }
                }
                //Attach item to hand
                closestBeam.transform.parent = transform;
                closestBeam.transform.localPosition = Vector3.zero;
                closestBeam.transform.localRotation = Quaternion.identity;
            }
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Beam>())
        {
            Beam beam = collision.GetComponent<Beam>();
            if (!beams.Contains(beam))
            {
                beams.Add(beam);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Beam>())
        {
            Beam beam = collision.GetComponent<Beam>();
            if (beams.Contains(beam))
            {
                beams.Remove(beam);
            }
        }
    }

}
