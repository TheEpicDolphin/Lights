using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (IsInPolygon(transform.position, beam.GetBeamPolygon(), true))
            {
                Debug.Log(beam.name);
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

    
    public bool IsInPolygon(Vector2 v, Vector2[] p, bool counterClockwise = true)
    {
        if (counterClockwise)
        {
            System.Array.Reverse(p);
        }

        //Points are clockwise
        int j = p.Length - 1;
        bool c = false;
        for (int i = 0; i < p.Length; j = i++)
        {
            c ^= p[i].y > v.y ^ p[j].y > v.y && v.x < (p[j].x - p[i].x) * (v.y - p[i].y) / (p[j].y - p[i].y) + p[i].x;
        }
        return c;

    }
}
