using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/*
public class Triangle
{
    public VertexNode[] vNodes;
}

//Face is on the right side of HalfEdge
public class HalfEdge
{
    public VertexNode vNode;
    public Triangle tri;

    public HalfEdge(VertexNode vNode)
    {
        this.vNode = vNode;
        this.tri = null;
    }

    public HalfEdge(VertexNode vNode, Triangle tri)
    {
        this.vNode = vNode;
        this.tri = tri;
    }
}

public class VertexNode
{
    public int vi;
    public List<HalfEdge> edges { get; private set; }

    public VertexNode(int vi)
    {
        this.vi = vi;
        this.edges = new List<HalfEdge>();
    }

    public VertexNode(int vi, List<HalfEdge> edges)
    {
        this.vi = vi;
        this.edges = edges;
    }

    public void SetEdges(List<HalfEdge> edges)
    {
        this.edges = edges;
    }
}

    
    private VertexNode DivideAndConquer(int s, int e)
    {
        if(e - s == 2)
        {
            HalfEdge e12 = new HalfEdge() 

            VertexNode vNode1 = new VertexNode(s);
            VertexNode vNode2 = new VertexNode(s + 1);
            HalfEdge e12 = new HalfEdge(vNode2);
            HalfEdge e21 = new HalfEdge(vNode1);
            vNode1.SetEdges(new List<HalfEdge> { e12 });
            vNode2.SetEdges(new List<HalfEdge> { e21 });
            return verts[vNode1.vi].y <= verts[vNode2.vi].y ? vNode1 : vNode2;
        }
        else if(e - s == 3)
        {
            VertexNode vNode1 = new VertexNode(s);
            VertexNode vNode2 = new VertexNode(s + 1);
            VertexNode vNode3 = new VertexNode(s + 2);
            HalfEdge e12 = new HalfEdge(vNode2);
            HalfEdge e21 = new HalfEdge(vNode1);
            vNode1.SetEdges(new List<HalfEdge> { e12 });
            vNode2.SetEdges(new List<HalfEdge> { e21 });
            return verts[vNode1.vi].y <= verts[vNode2.vi].y ? vNode1 : vNode2;
        }

        int m = (s + e) / 2;
        VertexNode leftTriangulation = DivideAndConquer(s, m);
        VertexNode rightTriangulation = DivideAndConquer(m, e);

    }
    
*/

public class Triangle
{
    public HalfEdge[] edges;
}

//Face is on the right side of HalfEdge
public class HalfEdge
{
    public int vi;
    public Triangle tri;
    public List<HalfEdge> edges { get; private set; }

    public HalfEdge(int vi)
    {
        this.vNode = vNode;
        this.tri = null;
    }

    public HalfEdge(VertexNode vNode, Triangle tri)
    {
        this.vNode = vNode;
        this.tri = tri;
    }

    public void SetEdges(List<HalfEdge> edges)
    {
        this.edges = edges;
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
        verts = verts.OrderBy(v => v.x).ThenBy(v => v.y).ToList();
        List<int> indices = new List<int>();
        HalfEdge triangulationRoot = DivideAndConquer(0, verts.Count);

    }

    /*
     *  Returns Root VertexNode, which has smallest y coordinate
     */ 
    
    private HalfEdge DivideAndConquer(int s, int e)
    {
        if(e - s == 2)
        {
            HalfEdge e01 = new HalfEdge(s);
            HalfEdge e10 = new HalfEdge(s + 1);
            e01.SetEdges(new List<HalfEdge> { e10 });
            e10.SetEdges(new List<HalfEdge> { e01 });
            return verts[e01.vi].y <= verts[e10.vi].y ? e01 : e10;
        }
        else if(e - s == 3)
        {
            Vector2 v0 = verts[s];
            Vector2 v1 = verts[s + 1];
            Vector2 v2 = verts[s + 2];
        }

        int m = (s + e) / 2;
        HalfEdge leftTriangulationRoot = DivideAndConquer(s, m);
        HalfEdge rightTriangulationRoot = DivideAndConquer(m, e);

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