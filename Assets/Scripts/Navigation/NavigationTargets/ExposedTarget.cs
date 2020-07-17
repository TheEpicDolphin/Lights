using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposedTarget : INavTarget
{
    Player player;
    Vector2 p;
    public ExposedTarget(Player player, Vector2 p)
    {
        this.p = p;
        this.player = player;
    }

    public bool IsValid()
    {
        return player.FOVContains(p);
    }

    public Vector2 Position()
    {
        return p;
    }
}
