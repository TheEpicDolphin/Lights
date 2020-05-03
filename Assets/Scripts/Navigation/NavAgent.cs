using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INavAgent
{
    Vector2 NavigateFromTo(Vector2 start, Vector2 destination, float radius)
    {
        if (Physics.Raycast())
        {
            //Find current navigation mesh
            Vector2 nextPoint = GetNextPointOnShortestPath(start, destination, radius);
        }
    }
}
