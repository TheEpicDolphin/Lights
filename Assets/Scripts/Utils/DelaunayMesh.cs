using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VecUtils;
using GeometryUtils;

//Traverse edges counterclockwise
public class Triangle
{
    private Vector2[] ghostBounds;
    public HalfEdge edge;
    public List<Triangle> children;

    /*
     * Doesn't modify the twins of the edges
     */
    public Triangle(HalfEdge e01, HalfEdge e12, HalfEdge e20)
    {
        e01.next = e12;
        e12.next = e20;
        e20.next = e01;

        e01.prev = e20;
        e20.prev = e12;
        e12.prev = e01;

        e01.incidentTriangle = this;
        e12.incidentTriangle = this;
        e20.incidentTriangle = this;

        this.edge = e01;
        this.children = new List<Triangle>();
        this.ghostBounds = GetCounterClockwiseVerts();
    }

    public Triangle FindContainingTriangle(Vector2 v)
    {
        if (children.Count == 0)
        {
            return this;
        }

        foreach (Triangle child in children)
        {

            if (child.Contains(v))
            {
                return child.FindContainingTriangle(v);
            }
        }

        //Shouldn't reach here
        return null;
    }

    private bool Contains(Vector2 p)
    {
        //Debug.Log(ghostBounds[0].ToString("F4") + ", " + ghostBounds[1].ToString("F4") + ", " + ghostBounds[2].ToString("F4"));
        return Geometry.IsInTriangle(ghostBounds[0], ghostBounds[1], ghostBounds[2], p);
    }

    public Vector2[] GetCounterClockwiseVerts()
    {
        Vector2[] verts = new Vector2[3];
        HalfEdge e = edge;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = e.origin.p;
            e = e.next;
        }
        return verts;
    }

    public bool IsImaginary()
    {
        HalfEdge e = this.edge;
        for (int i = 0; i < 3; i++)
        {
            if (e.origin.i == -1)
            {
                return true;
            }
            e = e.next;
        }
        return false;
    }
}

//Face is on the left side of the Half-Edge
public class HalfEdge
{
    public Vertex origin;
    public HalfEdge twin;
    public Triangle incidentTriangle;
    public HalfEdge next;
    public HalfEdge prev;

    public HalfEdge(Vertex v)
    {
        this.origin = v;
        this.twin = null;
        this.incidentTriangle = null;
        this.next = null;
        this.prev = null;
    }

    public static void SetTwins(HalfEdge e0, HalfEdge e1)
    {
        e0.twin = e1;
        e1.twin = e0;
    }

}

public class Vertex
{
    public Vector2 p;
    public int i;
    public Vertex(Vector2 p, int i)
    {
        this.p = p;
        this.i = i;
    }
}

public class DelaunayMesh
{
    private Vector2[] verts;
    public Triangle[] tris;
    public Triangle treeRoot;

    public DelaunayMesh(Vector2[] points)
    {
        verts = points;
        tris = Triangulate();
    }

    /*
    public NavMeshTriangle ContainingNavMeshTriangle(Vector2 p)
    {

    }
    */

    private Triangle[] Triangulate()
    {
        float[] xBounds = new float[2];
        float[] yBounds = new float[2];
        foreach (Vector2 vert in verts)
        {
            xBounds[0] = Mathf.Min(xBounds[0], vert.x);
            xBounds[1] = Mathf.Max(xBounds[1], vert.x);
            yBounds[0] = Mathf.Min(yBounds[0], vert.y);
            yBounds[1] = Mathf.Max(yBounds[1], vert.y);
        }

        //float LARGE_NUMBER = 10000.0f;
        //Vector2 pi0 = new Vector2(-LARGE_NUMBER, -LARGE_NUMBER);
        //Vector2 pi1 = new Vector2(LARGE_NUMBER, -LARGE_NUMBER);
        //Vector2 pi2 = new Vector2(0, 2 * LARGE_NUMBER);

        Vector2 pi0 = new Vector2(xBounds[0] - 0.5f, yBounds[0] - 0.5f);
        Vector2 pi1 = new Vector2(xBounds[1] + (xBounds[1] - xBounds[0]) + 2 * 0.5f, pi0.y);
        Vector2 pi2 = new Vector2(pi0.x, yBounds[1] + (yBounds[1] - yBounds[0]) + 2 * 0.5f);
        //Debug.DrawLine(pi0, pi1, Color.cyan, 5.0f, false);
        //Debug.DrawLine(pi1, pi2, Color.cyan, 5.0f, false);
        //Debug.DrawLine(pi2, pi0, Color.cyan, 5.0f, false);
        Vertex vi0 = new Vertex(pi0, -1);
        Vertex vi1 = new Vertex(pi1, -1);
        Vertex vi2 = new Vertex(pi2, -1);

        //Imaginary triangle
        HalfEdge e01 = new HalfEdge(vi0);
        HalfEdge e12 = new HalfEdge(vi1);
        HalfEdge e20 = new HalfEdge(vi2);
        Triangle treeRoot = new Triangle(e01, e12, e20);

        for (int i = 0; i < verts.Length; i++)
        {
            Vertex v = new Vertex(verts[i], i);
            Triangle containingTri = treeRoot.FindContainingTriangle(v.p);

            e01 = containingTri.edge;
            e12 = e01.next;
            e20 = e12.next;

            HalfEdge e13 = new HalfEdge(e12.origin);
            HalfEdge e30 = new HalfEdge(v);
            Triangle tri0 = new Triangle(e01, e13, e30);

            HalfEdge e23 = new HalfEdge(e20.origin);
            HalfEdge e31 = new HalfEdge(v);
            Triangle tri1 = new Triangle(e12, e23, e31);

            HalfEdge e03 = new HalfEdge(e01.origin);
            HalfEdge e32 = new HalfEdge(v);
            Triangle tri2 = new Triangle(e20, e03, e32);

            HalfEdge.SetTwins(e03, e30);
            HalfEdge.SetTwins(e13, e31);
            HalfEdge.SetTwins(e23, e32);

            containingTri.children = new List<Triangle> { tri0, tri1, tri2 };

            //Flip triangles that don't satisfy delaunay property
            HashSet<HalfEdge> sptSet = new HashSet<HalfEdge>();
            LinkedList<HalfEdge> frontier = new LinkedList<HalfEdge>();
            frontier.AddLast(e01);
            frontier.AddLast(e12);
            frontier.AddLast(e20);

            LinkedListNode<HalfEdge> curNode;
            int t = 0;
            while (frontier.Count > 0 && t++ < 100)
            {
                curNode = frontier.First;
                frontier.RemoveFirst();
                HalfEdge ab = curNode.Value;
                HalfEdge ba = ab.twin;
                if (!sptSet.Contains(ab) && ba != null)
                {
                    HalfEdge bc = ab.next;
                    HalfEdge ca = bc.next;

                    HalfEdge ad = ba.next;
                    HalfEdge db = ad.next;
                    if (Geometry.IsInCircumscribedCircle(ab.origin.p, bc.origin.p, ca.origin.p, db.origin.p))
                    {
                        HalfEdge dc = new HalfEdge(db.origin);
                        HalfEdge cd = new HalfEdge(ca.origin);
                        HalfEdge.SetTwins(dc, cd);
                        Triangle adc = new Triangle(ad, dc, ca);
                        Triangle bcd = new Triangle(bc, cd, db);

                        ab.incidentTriangle.children = new List<Triangle> { adc, bcd };
                        ba.incidentTriangle.children = new List<Triangle> { adc, bcd };
                    }

                    frontier.AddLast(ad);
                    frontier.AddLast(db);
                    sptSet.Add(ab);
                    sptSet.Add(ba);
                }
            }
        }

        //Generate Triangle list
        List<Triangle> leafs = new List<Triangle>();
        HashSet<Triangle> leafSet = new HashSet<Triangle>();
        GetRealLeafs(treeRoot, ref leafs, ref leafSet);
        
        foreach(Triangle leaf in leafs)
        {
            Vector2 p0 = leaf.edge.origin.p;
            Vector2 p1 = leaf.edge.next.origin.p;
            Vector2 p2 = leaf.edge.next.next.origin.p;
            Debug.DrawLine(p0, p1, Color.cyan, 5.0f, false);
            Debug.DrawLine(p1, p2, Color.cyan, 5.0f, false);
            Debug.DrawLine(p2, p0, Color.cyan, 5.0f, false);
        }
        
        return leafs.ToArray();
    }



    private void GetRealLeafs(Triangle node, ref List<Triangle> leafs, ref HashSet<Triangle> leafSet)
    {
        if(node.children.Count == 0 && !leafSet.Contains(node) && !node.IsImaginary())
        {
            leafs.Add(node);
            leafSet.Add(node);
        }

        foreach(Triangle child in node.children)
        {
            GetRealLeafs(child, ref leafs, ref leafSet);
        }

    }
}
