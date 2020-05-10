using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VecUtils;
using GeometryUtils;

//Traverse edges counterclockwise
public class Triangle
{
    public HalfEdge edge;

    //Children are entirely contained within parent
    public List<Triangle> children;

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
    }

    public Triangle FindContainingTriangle(Vector2 v)
    {
        if(children.Count == 0)
        {
            return this;
        }

        foreach(Triangle child in children)
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
        Vector2[] verts = GetCounterClockwiseVerts();
        return Geometry.IsInTriangle(verts[0], verts[1], verts[2], p);
    }

    public Vector2[] GetCounterClockwiseVerts()
    {
        Vector2[] verts = new Vector2[3];
        HalfEdge e = edge;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = edge.origin;
            e = e.next;
        }
        return verts;
    }

}


//Face is on the left side of the Half-Edge
public class HalfEdge
{
    public Vector2 origin;
    public HalfEdge twin;
    public Triangle incidentTriangle;
    public HalfEdge next;
    public HalfEdge prev;

    public HalfEdge(Vector2 v)
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
    public Vector2 v;
    public HalfEdge incidentEdge;

    public Vertex(Vector2 v, HalfEdge incidentEdge)
    {
        this.v = v;
        this.incidentEdge = incidentEdge;
    }

    public Vertex(Vector2 v)
    {
        this.v = v;
        this.incidentEdge = null;
    }
}

public class Triangulator
{
    private List<Vector2> verts = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        verts = new List<Vector2>(points);
    }

    public int[] DelaunayTriangulate()
    {
        float[] xBounds = new float[2];
        float[] yBounds = new float[2];
        foreach(Vector2 vert in verts)
        {
            xBounds[0] = Mathf.Min(xBounds[0], vert.x);
            xBounds[1] = Mathf.Max(xBounds[1], vert.x);
            yBounds[0] = Mathf.Min(yBounds[0], vert.y);
            yBounds[1] = Mathf.Max(yBounds[1], vert.y);
        }
        List<int> indices = new List<int>();

        Vector2 v0 = new Vector2(xBounds[0], yBounds[0]);
        Vector2 v1 = v0 + 2 * new Vector2(xBounds[1], 0);
        Vector2 v2 = v0 + 2 * new Vector2(0, yBounds[1]);
        HalfEdge e01 = new HalfEdge(v0);
        HalfEdge e12 = new HalfEdge(v1);
        HalfEdge e20 = new HalfEdge(v2);
        Triangle treeRoot = new Triangle(e01, e12, e20);

        for(int i = 0; i < verts.Count; i++)
        {
            Vector2 v = verts[i];
            Triangle containingTri = treeRoot.FindContainingTriangle(v);

            /*
            HalfEdge pEdge = containingTri.edge;
            Triangle[] newTris = new Triangle[3];
            for(int t = 0; t < newTris.Length; t++)
            {
                Vector2 vorigin = pEdge.origin;
                HalfEdge e13 = new HalfEdge(pEdge.next.origin);
                HalfEdge e30 = new HalfEdge(v);
                newTris[t] = new Triangle(pEdge, e13, e30);
                pEdge = pEdge.next;
            }
            HalfEdge.SetTwins(e03, e30);
            HalfEdge.SetTwins(e13, e31);
            HalfEdge.SetTwins(e23, e32);
            */

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


            //Flip triangles that don't satisfy property delaunay property
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
                if (!sptSet.Contains(curNode.Value))
                {

                    if (Geometry.IsInCircumscribedCircle())
                    {
                        List<int> neighbors = this.nodes[curNode.Value].GetNeighbors();
                        for (int i = 0; i < neighbors.Count; i++)
                        {
                            int neighbor = neighbors[i];

                            if (!sptSet.Contains(neighbor))
                            {
                                frontier.AddLast(neighbor);
                            }
                        }
                        sptSet.Add(curNode.Value);
                    }
                }
                t += 1;

            }


        }

    }
    

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = verts.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];

                indices.Add(a);
                indices.Add(b);
                indices.Add(c);

                //indices.Add(c);
                //indices.Add(b);
                //indices.Add(a);
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = verts.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = verts[p];
            Vector2 qval = verts[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = verts[V[u]];
        Vector2 B = verts[V[v]];
        Vector2 C = verts[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = verts[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}