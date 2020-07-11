using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmark : INavTarget
{
    public Vector2 p;
    public Landmark(Vector2 p)
    {
        this.p = p;
    }

    public bool IsValid()
    {
        //TODO: fix this
        return true;
    }
}
