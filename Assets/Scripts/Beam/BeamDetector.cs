using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamDetector : MonoBehaviour
{
    LightTarget lightTarget;
    // Start is called before the first frame update
    void Start()
    {
        lightTarget = GetComponentInParent<LightTarget>();
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        //col.offset = new Vector2((beam.xLims[1] + beam.xLims[0]) / 2.0f, beam.beamLength / 2.0f);
        //col.size = new Vector2(beam.xLims[1] - beam.xLims[0], beam.beamLength);
    }

    private void FixedUpdate()
    {

    }
    
}
