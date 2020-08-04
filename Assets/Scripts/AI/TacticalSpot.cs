using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct TacticalSpot
{
    public Vector2 origin;
    public Vector2[] path;

    public TacticalSpot(Vector2 origin, Vector2[] path)
    {
        this.origin = origin;
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

    public float Distance()
    {
        float d = Vector2.Distance(origin, FirstPathPoint());
        for(int i = 0; i < path.Length - 1; i++)
        {
            d += Vector2.Distance(path[i], path[i + 1]);
        }
        return d;
    }
}
