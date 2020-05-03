using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INavAgent
{
    Vector2[] GetShortestPathFromTo(Vector2 start, Vector2 destination, float radius);
}
