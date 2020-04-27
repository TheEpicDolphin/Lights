using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class Reflector : Obstacle
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
        Vector2 lims0Cur = beamLocalToCur.MultiplyPoint3x4(limsBeamLocal[0]);
        Vector2 lims1Cur = beamLocalToCur.MultiplyPoint3x4(limsBeamLocal[1]);

        // do transformations...
        Vector2 n = Vector2.Perpendicular(lims1Cur - lims0Cur).normalized;
        Matrix4x4 Mrefl = Geometry.ReflectTransformAcrossPlane(n, lims0Cur, beamLocalToCur);

        Vector2 lims0World = beam.transform.TransformPoint(limsBeamLocal[0]);
        Vector2 lims1World = beam.transform.TransformPoint(limsBeamLocal[1]);
        Vector2 sourceWorld = (lims0World + lims1World) / 2;
        Vector2 dirWorld = beam.transform.TransformDirection(Mrefl.inverse.MultiplyVector(new Vector2(0, 1)));
        float beamWidth = (lims1World - lims0World).magnitude / 2;
        List<Obstacle> obstacles = beam.GetObstaclesInBeam(sourceWorld, dirWorld, beamWidth, beamLength, this);

        Vector2[] limsRefl = new Vector2[] { Mrefl.MultiplyPoint3x4(limsBeamLocal[0]),
                                            Mrefl.MultiplyPoint3x4(limsBeamLocal[1]) };

        //TODO: reverse vertices because of reflection
        beam.Cast(limsRefl, obstacles, Mrefl, beamLength, maxRecurse, ref beamComponents);
    }
    
}
