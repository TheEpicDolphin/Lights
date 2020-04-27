using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Obstacle : MonoBehaviour
{
    EdgeCollider2D edgeCol;

    // Start is called before the first frame update
    protected void Start()
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

    public Vector2[] GetWorldBoundVerts()
    {
        Vector2[] verts = new Vector2[edgeCol.points.Length - 1];
        for(int i = 0; i < edgeCol.points.Length - 1; i++)
        {
            verts[i] = transform.TransformPoint(edgeCol.points[i]);
        }
        return verts;
    }

    public Vector2[] GetLocalBoundVerts(Matrix4x4 M, bool clockwise = false)
    {
        Vector2[] verts = new Vector2[edgeCol.points.Length - 1];
        for (int i = 0; i < edgeCol.points.Length - 1; i++)
        {
            verts[i] = M.MultiplyPoint(transform.TransformPoint(edgeCol.points[i]));
        }
        if (clockwise)
        {
            System.Array.Reverse(verts);
        }
        return verts;
    }

    
    public abstract void Cast(Beam beam, Vector2[] lims, Matrix4x4 beamLocalToCur, 
                                            float beamLength, int maxRecurse, ref List<List<Vector2>> beamComponents);
    
}

