using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Obstacle : MonoBehaviour
{
    EdgeCollider2D edgeCol;


    // Start is called before the first frame update
    void Start()
    {
        edgeCol = GetComponent<EdgeCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {

    }

    public Vector2[] GetBoundVerts()
    {
        Vector2[] verts = new Vector2[edgeCol.points.Length - 1];
        for(int i = 0; i < edgeCol.points.Length - 1; i++)
        {
            verts[i] = transform.TransformPoint(edgeCol.points[i]);
        }
        return verts;
    }

}

