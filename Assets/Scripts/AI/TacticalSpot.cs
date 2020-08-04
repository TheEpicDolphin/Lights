using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct TacticalSpot
{
    public Vector2[] path;

    public TacticalSpot(Vector2[] path)
    {
        this.path = path;
    }

    public Vector2 FirstPathPoint()
    {
        return path.First();
    }

    public Vector2 Position()
    {
        return path.Last();
    }
}
