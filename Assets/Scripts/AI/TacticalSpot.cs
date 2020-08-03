using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct TacticalSpot
{
    public List<Vector2> path;

    public TacticalSpot(List<Vector2> path)
    {
        this.path = path;
    }

    public Vector2 Position()
    {
        return path.Last();
    }
}
