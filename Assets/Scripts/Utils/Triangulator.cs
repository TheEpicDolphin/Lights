using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VecUtils;

/*
//Vertices are clockwise
public class Triangle
{
    public HalfEdge[] edges;

    public Triangle(HalfEdge[] edges)
    {
        this.edges = edges;
    }

    public Delete()
    {
        foreach(HalfEdge edge in edges)
        {
            edge.tri = null;
        }
    }
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

//Edges are in clockwise order around vertex
public class VertexNode
{
    public int i;
    public LinkedList<HalfEdge> edges { get; private set; }

    public VertexNode(int i)
    {
        this.i = i;
        this.edges = new LinkedList<HalfEdge>();
    }

    public VertexNode(int vi, LinkedList<HalfEdge> edges)
    {
        this.i = i;
        this.edges = edges;
    }

    public void SetEdges(LinkedList<HalfEdge> edges)
    {
        this.edges = edges;
    }

}

        private VertexNode DivideAndConquer(int s, int e)
    {
        if (e - s == 2)
        {
            VertexNode v0 = new VertexNode(s);
            VertexNode v1 = new VertexNode(s + 1);
            HalfEdge e01 = new HalfEdge(v1);
            HalfEdge e10 = new HalfEdge(v0);
            v0.edges.AddAfter(e01);
            v1.edges.AddAfter(e10);
            return verts[v0.i].y <= verts[v1.i].y ? v0 : v1;
        }
        else if(e - s == 3)
        {
            VertexNode v0;
            VertexNode v1;
            VertexNode v2;
            if (verts[s].y <= Mathf.Min(verts[s + 1].y, verts[s + 2].y))
            {
                v0 = new VertexNode(s);
                if (VecMath.Det(verts[s + 1] - verts[s], verts[s + 2] - verts[s]) <= 0)
                {
                    v1 = new VertexNode(s + 1);
                    v2 = new VertexNode(s + 2);
                }
                else
                {
                    v1 = new VertexNode(s + 2);
                    v2 = new VertexNode(s + 1);
                }
            }
            else if(verts[s + 1].y <= verts[s + 2].y)
            {
                v0 = new VertexNode(s + 1);
                if (VecMath.Det(verts[s] - verts[s + 1], verts[s + 2] - verts[s + 1]) <= 0)
                {
                    v1 = new VertexNode(s);
                    v2 = new VertexNode(s + 2);
                }
                else
                {
                    v1 = new VertexNode(s + 2);
                    v2 = new VertexNode(s);
                }
            }
            else
            {
                v0 = new VertexNode(s + 2);
                if (VecMath.Det(verts[s] - verts[s + 2], verts[s + 1] - verts[s + 2]) <= 0)
                {
                    v1 = new VertexNode(s);
                    v2 = new VertexNode(s + 1);
                }
                else
                {
                    v1 = new VertexNode(s + 1);
                    v2 = new VertexNode(s);
                }
            }
            Triangle tri = new Triangle(new VertexNode[] { v0, v1, v2 });
            HalfEdge e01 = new HalfEdge(v1, tri);
            HalfEdge e12 = new HalfEdge(v2, tri);
            HalfEdge e20 = new HalfEdge(v0, tri);
            v0.edges.AddFirst(e01);
            v1.edges.AddFirst(e12);
            v2.edges.AddFirst(e20);
            HalfEdge e02 = new HalfEdge(v2, null);
            HalfEdge e21 = new HalfEdge(v1, null);
            HalfEdge e10 = new HalfEdge(v0, null);
            v0.edges.AddLast(e02);
            v2.edges.AddLast(e21);
            v1.edges.AddLast(e10);
            return v0;
        }

        int m = (s + e) / 2;
        VertexNode baseL = DivideAndConquer(s, m);
        VertexNode baseR = DivideAndConquer(m, e);

        //Find base LR edge
        VertexNode potentialCandidateL = baseL.edges.Last.Value;
        VertexNode potentialCandidateR = baseR.edges.First.Value;
        while (VecMath.Det(verts[potentialCandidateL.i] - verts[baseL.i],
                verts[baseR.i] - verts[baseL.i]))
        {
            baseL = potentialCandidateL;
            potentialCandidateL = baseL.edges.Last.Value;
        }


    }
*/

//Vertices are clockwise
public class Triangle : LinkedListNode<VertexNode>
{
    public VertexNode NextVertex()
    {
        
    }
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

//Edges are in clockwise order around vertex
public class VertexNode
{
    public int i;
    public LinkedList<LinkedListNode<VertexNode>> triangles { get; private set; }

    public VertexNode(int i)
    {
        this.i = i;
        this.edges = new LinkedList<HalfEdge>();
    }

    public VertexNode(int vi, LinkedList<HalfEdge> edges)
    {
        this.i = i;
        this.edges = edges;
    }

    public void SetEdges(LinkedList<HalfEdge> edges)
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
        VertexNode triangulationRoot = DivideAndConquer(0, verts.Count);

    }

    /*
     *  Returns Root VertexNode, which has smallest y coordinate
     */ 
    
    private VertexNode DivideAndConquer(int s, int e)
    {
        if (e - s == 2)
        {
            VertexNode v0 = new VertexNode(s);
            VertexNode v1 = new VertexNode(s + 1);
            HalfEdge e01 = new HalfEdge(v1);
            HalfEdge e10 = new HalfEdge(v0);
            v0.edges.AddAfter(e01);
            v1.edges.AddAfter(e10);
            return verts[v0.i].y <= verts[v1.i].y ? v0 : v1;
        }
        else if(e - s == 3)
        {
            VertexNode v0;
            VertexNode v1;
            VertexNode v2;
            if (verts[s].y <= Mathf.Min(verts[s + 1].y, verts[s + 2].y))
            {
                v0 = new VertexNode(s);
                if (VecMath.Det(verts[s + 1] - verts[s], verts[s + 2] - verts[s]) <= 0)
                {
                    v1 = new VertexNode(s + 1);
                    v2 = new VertexNode(s + 2);
                }
                else
                {
                    v1 = new VertexNode(s + 2);
                    v2 = new VertexNode(s + 1);
                }
            }
            else if(verts[s + 1].y <= verts[s + 2].y)
            {
                v0 = new VertexNode(s + 1);
                if (VecMath.Det(verts[s] - verts[s + 1], verts[s + 2] - verts[s + 1]) <= 0)
                {
                    v1 = new VertexNode(s);
                    v2 = new VertexNode(s + 2);
                }
                else
                {
                    v1 = new VertexNode(s + 2);
                    v2 = new VertexNode(s);
                }
            }
            else
            {
                v0 = new VertexNode(s + 2);
                if (VecMath.Det(verts[s] - verts[s + 2], verts[s + 1] - verts[s + 2]) <= 0)
                {
                    v1 = new VertexNode(s);
                    v2 = new VertexNode(s + 1);
                }
                else
                {
                    v1 = new VertexNode(s + 1);
                    v2 = new VertexNode(s);
                }
            }
            Triangle tri = new Triangle(new VertexNode[] { v0, v1, v2 });
            HalfEdge e01 = new HalfEdge(v1, tri);
            HalfEdge e12 = new HalfEdge(v2, tri);
            HalfEdge e20 = new HalfEdge(v0, tri);
            v0.edges.AddFirst(e01);
            v1.edges.AddFirst(e12);
            v2.edges.AddFirst(e20);
            HalfEdge e02 = new HalfEdge(v2, null);
            HalfEdge e21 = new HalfEdge(v1, null);
            HalfEdge e10 = new HalfEdge(v0, null);
            v0.edges.AddLast(e02);
            v2.edges.AddLast(e21);
            v1.edges.AddLast(e10);
            return v0;
        }

        int m = (s + e) / 2;
        VertexNode baseL = DivideAndConquer(s, m);
        VertexNode baseR = DivideAndConquer(m, e);

        //Find base LR edge
        VertexNode potentialCandidateL = baseL.edges.Last.Value;
        VertexNode potentialCandidateR = baseR.edges.First.Value;
        while (VecMath.Det(verts[potentialCandidateL.i] - verts[baseL.i],
                verts[baseR.i] - verts[baseL.i]))
        {
            baseL = potentialCandidateL;
            potentialCandidateL = baseL.edges.Last.Value;
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