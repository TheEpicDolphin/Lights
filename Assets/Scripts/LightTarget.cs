using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class LightTarget : MonoBehaviour
{

    BeamDetector beamDetector;
    List<Beam> potentialBeams = new List<Beam>();

    // Start is called before the first frame update
    void Start()
    {
        potentialBeams = new List<Beam>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Beam beam in potentialBeams)
        {
            if (Geometry.IsInPolygon(transform.position, beam.GetBeamPolygon(), true))
            {
                //Debug.Log(beam.name);
            }
        }

        potentialBeams = new List<Beam>();
    }

    public void AddPotentialBeam(Beam beam)
    {
        if (!potentialBeams.Contains(beam))
        {
            potentialBeams.Add(beam);
        }
    }

    
    
}
