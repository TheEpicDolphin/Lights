﻿using System.Collections;
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

    
    public override List<List<Vector2>> Cast(Beam beam, Vector2[] limsBeamLocal, Matrix4x4 beamLocalToCur, float beamLength, int maxRecurse)
    {
        Vector2 lims0Cur = beamLocalToCur.MultiplyPoint3x4(limsBeamLocal[0]);
        Vector2 lims1Cur = beamLocalToCur.MultiplyPoint3x4(limsBeamLocal[1]);

        // do transformations...
        Vector2 n = Vector2.Perpendicular(lims1Cur - lims0Cur).normalized;
        Matrix4x4 Mrefl = Geometry.ReflectTransformAcrossPlane(n, lims0Cur, beamLocalToCur);

        Vector2 lims0World = beam.transform.TransformPoint(limsBeamLocal[0]);
        Vector2 lims1World = beam.transform.TransformPoint(limsBeamLocal[1]);
        Vector2 sourceWorld = (lims0World + lims1World) / 2;
        Vector2 dirWorld = beam.transform.TransformDirection(Mrefl.inverse.MultiplyVector(Mrefl.GetColumn(1)));
        float beamWidth = (lims1World - lims0World).magnitude / 2;
        List<Obstacle> obstacles = beam.GetObstaclesInBeam(sourceWorld, dirWorld, beamWidth, beamLength, this);

        Vector2[] limsRefl = new Vector2[] { Mrefl.MultiplyPoint3x4(limsBeamLocal[0]),
                                            Mrefl.MultiplyPoint3x4(limsBeamLocal[1]) };

        //Debug.Log("ZOOWEE");
        //Debug.Log(limsRefl[0]);
        //Debug.Log(limsRefl[1]);

        //TODO: reverse vertices because of reflection
        return beam.Cast(limsRefl, obstacles, Mrefl, beamLength, maxRecurse);
    }
    
}
