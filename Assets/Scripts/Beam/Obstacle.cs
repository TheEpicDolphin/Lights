using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlgorithmUtils;


public abstract class Obstacle : MonoBehaviour
{
    protected EdgeCollider2D edgeCol;

    // Start is called before the first frame update
    protected void Start()
    {
        edgeCol = GetComponent<EdgeCollider2D>();
    }

    public void InitializeEdges()
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

    private int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public Vector2[] GetWorldMinkowskiBoundVerts(float radius, bool clockwise = false)
    {
        Vector2[] boundVerts = GetWorldBoundVerts();
        Vector2[] minkowskiBoundVerts = new Vector2[boundVerts.Length];
        for(int i = 0; i < boundVerts.Length; i++)
        {
            Vector2 n1 = -Vector2.Perpendicular(boundVerts[i] - boundVerts[Mod(i - 1, boundVerts.Length)]);
            Vector2 n2 = -Vector2.Perpendicular(boundVerts[Mod(i + 1, boundVerts.Length)] - boundVerts[i]);
            Vector2 n = (n1 + n2).normalized;
            minkowskiBoundVerts[i] = boundVerts[i] + radius * n;
        }
        if (clockwise)
        {
            System.Array.Reverse(minkowskiBoundVerts);
        }
        return minkowskiBoundVerts;
    }

    public Vector2[] GetWorldBoundVerts(bool clockwise = false)
    {
        Vector2[] verts = new Vector2[edgeCol.points.Length - 1];
        for(int i = 0; i < edgeCol.points.Length - 1; i++)
        {
            verts[i] = transform.TransformPoint(edgeCol.points[i]);
        }
        if (clockwise)
        {
            System.Array.Reverse(verts);
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

    
    public abstract void Cast(Beam beam, Vector2[] limsBeamLocal, Matrix4x4 beamLocalToCur, 
                                            float beamLength, int maxRecurse, ref List<List<Vector2>> beamComponents);
    
}

