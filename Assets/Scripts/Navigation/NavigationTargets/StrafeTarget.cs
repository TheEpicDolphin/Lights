using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrafeTarget : INavTarget
{
    Vector2 p;
    public StrafeTarget(Vector2 p)
    {
        this.p = p;
    }

    public bool IsValid()
    {
        return true;
    }

    public Vector2 Position()
    {
        return p;
    }
}
