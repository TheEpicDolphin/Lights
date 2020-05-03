using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filter : Obstacle
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Cast(Beam beam, Vector2[] limsBeamLocal, Matrix4x4 beamLocalToCur,
    float beamLength, int maxRecurse, ref List<List<Vector2>> beamComponents)
    {
        Vector2 lims0World = beam.transform.TransformPoint(limsBeamLocal[0]);
        Vector2 lims1World = beam.transform.TransformPoint(limsBeamLocal[1]);
        Vector2 sourceWorld = (lims0World + lims1World) / 2;
        Vector2 dirWorld = beam.transform.TransformDirection(new Vector2(0, 1));
        float beamWidth = (lims1World - lims0World).magnitude / 2;
        List<Obstacle> obstacles = beam.GetObstaclesInBeam(sourceWorld, dirWorld, beamWidth, beamLength, this);

        beam.Cast(limsBeamLocal, obstacles, beamLocalToCur, beamLength, maxRecurse, ref beamComponents);
    }
}
