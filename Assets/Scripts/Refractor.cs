using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class Refractor : Obstacle
{
    //Refractive index
    public float n2_n1 = 1.36f;
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
        Vector2 n = Vector2.Perpendicular(lims1Cur - lims0Cur).normalized;

        Vector2 vI = new Vector2(0, 1);
        Matrix4x4 Mrefr = Geometry.RefractTransformWithPlane(n, lims0Cur, vI, n2_n1) * beamLocalToCur;

        Vector3 right = Mrefr.GetColumn(0);
        Vector3 up = Mrefr.GetColumn(1);
        Vector3 p1 = Mrefr.GetColumn(3);
        Debug.DrawRay(beam.transform.TransformPoint(p1), beam.transform.TransformDirection(up), Color.yellow);
        //Debug.DrawRay(beam.transform.position, beam.transform.up, Color.green);

        //Vector3 o = beam.transform.TransformPoint(Mrefr.MultiplyPoint3x4(Vector3.zero));
        //Debug.DrawLine(Vector3.zero, o, Color.cyan);

        Vector2 lims0World = beam.transform.TransformPoint(limsBeamLocal[0]);
        Vector2 lims1World = beam.transform.TransformPoint(limsBeamLocal[1]);
        Vector2 sourceWorld = (lims0World + lims1World) / 2;
        Vector2 dirWorld = beam.transform.TransformDirection(Mrefr.inverse.MultiplyVector(vI));
        float beamWidth = (lims1World - lims0World).magnitude / 2;
        List<Obstacle> obstacles = beam.GetObstaclesInBeam(sourceWorld, dirWorld, beamWidth, beamLength, this);

        Vector2[] limsRefr = new Vector2[] { Mrefr.MultiplyPoint3x4(limsBeamLocal[0]),
                                            Mrefr.MultiplyPoint3x4(limsBeamLocal[1]) };

        //Cast refracted beam
        beam.Cast(limsRefr, obstacles, Mrefr, beamLength, maxRecurse, ref beamComponents);
    }
}
