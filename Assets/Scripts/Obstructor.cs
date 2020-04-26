using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstructor : Obstacle
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

    public override List<List<Vector2>> Cast(Beam beam, Vector2[] lims, Matrix4x4 beamLocalToCur, float beamLength, int maxRecurse)
    {
        return new List<List<Vector2>>();
    }
}
