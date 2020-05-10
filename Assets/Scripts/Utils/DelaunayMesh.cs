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

    //Children are entirely contained within parent
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
            verts[i] = e.origin;
            e = e.next;
        }
        return verts;
    }
}

//Face is on the left side of the Half-Edge
public class HalfEdge
{
    public Vector2 origin;
    public int origini;
    public HalfEdge twin;
    public Triangle incidentTriangle;
    public HalfEdge next;
    public HalfEdge prev;

    public HalfEdge(Vector2 v, int origini)
    {
        this.origin = v;
        this.origini = origini;
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
        List<int> indices = new List<int>();

        Vector2 v0 = new Vector2(xBounds[0] - 0.1f, yBounds[0] - 0.1f);
        Vector2 v1 = new Vector2(xBounds[1] + (xBounds[1] - xBounds[0]) + 0.1f, v0.y);
        Vector2 v2 = new Vector2(v0.x, yBounds[1] + (yBounds[1] - yBounds[0]) + 0.1f);

        //Imaginary triangle
        HalfEdge e01 = new HalfEdge(v0, -1);
        HalfEdge e12 = new HalfEdge(v1, -1);
        HalfEdge e20 = new HalfEdge(v2, -1);
        Triangle treeRoot = new Triangle(e01, e12, e20);
        //Debug.Log(v0.ToString("F4") + ", " + v1.ToString("F4") + ", " + v2.ToString("F4"));

        for (int i = 0; i < verts.Length; i++)
        {
            Vector2 v = verts[i];
            //Debug.Log(i);
            //Debug.Log(v);
            Triangle containingTri = treeRoot.FindContainingTriangle(v);
            //Debug.Log(containingTri);

            e01 = containingTri.edge;
            e12 = e01.next;
            e20 = e12.next;

            HalfEdge e13 = new HalfEdge(e12.origin, e12.origini);
            HalfEdge e30 = new HalfEdge(v, i);
            Triangle tri0 = new Triangle(e01, e13, e30);

            HalfEdge e23 = new HalfEdge(e20.origin, e20.origini);
            HalfEdge e31 = new HalfEdge(v, i);
            Triangle tri1 = new Triangle(e12, e23, e31);

            HalfEdge e03 = new HalfEdge(e01.origin, e01.origini);
            HalfEdge e32 = new HalfEdge(v, i);
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

            while (frontier.Count > 0 && t < 100)
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
                    if (Geometry.IsInCircumscribedCircle(ab.origin, bc.origin, ca.origin, db.origin))
                    {
                        HalfEdge dc = new HalfEdge(db.origin, db.origini);
                        HalfEdge cd = new HalfEdge(ca.origin, ca.origini);
                        HalfEdge.SetTwins(dc, cd);
                        Triangle adc = new Triangle(ad, dc, ca);
                        Triangle bcd = new Triangle(bc, cd, db);

                        ab.incidentTriangle.children = new List<Triangle> { adc, bcd };
                        ba.incidentTriangle.children = new List<Triangle> { adc, bcd };

                        frontier.AddLast(ad);
                        frontier.AddLast(db);
                        sptSet.Add(ab);
                        sptSet.Add(ba);
                    }
                }
                t += 1;
            }
        }

        //Generate Triangle list
        List<Triangle> leafs = new List<Triangle>();
        HashSet<Triangle> leafSet = new HashSet<Triangle>();
        GetRealLeafTriangles(treeRoot, ref leafs, ref leafSet);
        
        foreach(Triangle leaf in leafs)
        {
            Debug.Log(leaf.edge.origin.ToString("F4") + ", " +
                    leaf.edge.next.origin.ToString("F4") + ", " + 
                    leaf.edge.next.next.origin.ToString("F4"));
        }
        
        return leafs.ToArray();
    }

    private void GetRealLeafTriangles(Triangle node, ref List<Triangle> leafs, ref HashSet<Triangle> leafSet)
    {
        if(node.children.Count == 0 && !leafSet.Contains(node))
        {
            HalfEdge e = node.edge;
            for (int i = 0; i < 3; i++)
            {
                if(e.origini == -1)
                {
                    return;
                }
                e = e.next;
            }
            leafs.Add(node);
            leafSet.Add(node);
        }

        foreach(Triangle child in node.children)
        {
            GetRealLeafTriangles(child, ref leafs, ref leafSet);
        }

        
    }
}
