using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, INavAgent
{
    public NavigationMesh navMesh;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetShortestPathFromTo(transform.position, player.transform.position, 1.0f);
    }

    public Vector2[] GetShortestPathFromTo(Vector2 start, Vector2 destination, float radius)
    {
        return navMesh.GetShortestPathFromTo(start, destination, radius);
    }


}
