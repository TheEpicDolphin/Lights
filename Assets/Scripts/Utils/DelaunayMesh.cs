using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;

//Traverse edges counterclockwise
public class Triangle
{
    public HalfEdge edge;
    public List<HalfEdge> childrenBounds;

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
        this.childrenBounds = new List<HalfEdge>();
    }

    //Possibly return an edge rather than a triangle if point lies between to triangles
    public Triangle FindContainingTriangle(Vector2 v)
    {
        if (childrenBounds.Count == 0)
        {
            return this;
        }
        else if(childrenBounds.Count == 1)
        {
            //Two children
            Vector2 n = Vector2.Perpendicular(childrenBounds[0].twin.origin.p - childrenBounds[0].origin.p);
            if (Vector2.Dot(n, v - childrenBounds[0].origin.p) >= 0)
            {
                return childrenBounds[0].incidentTriangle.FindContainingTriangle(v);
            }
            else
            {
                return childrenBounds[0].twin.incidentTriangle.FindContainingTriangle(v);
            }
        }
        else
        {
            for(int i = 0; i < childrenBounds.Count; i++)
            {
                HalfEdge bound = childrenBounds[i];
                HalfEdge nextBound = childrenBounds[(i + 1) % childrenBounds.Count].twin;
                Vector2 n1 = Vector2.Perpendicular(bound.twin.origin.p - bound.origin.p);
                Vector2 n2 = Vector2.Perpendicular(nextBound.twin.origin.p - nextBound.origin.p);
                if (Vector2.Dot(n1, v - bound.origin.p) >= 0 && Vector2.Dot(n2, v - nextBound.origin.p) >= 0)
                {
                    return bound.incidentTriangle.FindContainingTriangle(v);
                }
            }
        }

        //Shouldn't reach here
        return null;
    }

    private bool Contains(Vector2 p)
    {
        //return Geometry.IsInTriangle(ghostBounds[0].p, ghostBounds[1].p, ghostBounds[2].p, p);
        return true;
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

    public void GetRealLeafs(ref List<Triangle> leafs, ref HashSet<Triangle> visited,
                            ref int count, int depth)
    {
        if (visited.Contains(this))
        {
            return;
        }
        count += 1;
        if (this.childrenBounds.Count == 0)
        {
            Debug.Log("Depth: " + depth.ToString());
            if (!this.IsImaginary())
            {
                leafs.Add(this);
            }
        }
        else if (this.childrenBounds.Count == 1)
        {
            HalfEdge bound = this.childrenBounds[0];
            bound.incidentTriangle.GetRealLeafs(ref leafs, ref visited, ref count, depth + 1);
            bound.twin.incidentTriangle.GetRealLeafs(ref leafs, ref visited, ref count, depth + 1);
        }
        else if (this.childrenBounds.Count == 3)
        {
            foreach (HalfEdge bound in this.childrenBounds)
            {
                bound.incidentTriangle.GetRealLeafs(ref leafs, ref visited, ref count, depth + 1);
            }
        }

        visited.Add(this);

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

//When doing constrained delaunay triangulations, give Vertex reference to HalfEdges
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
        Algorithm.Shuffle<Vector2>(ref points);
        verts = points;
        tris = Triangulate();
    }

    /*
    public NavMeshTriangle ContainingNavMeshTriangle(Vector2 p)
    {
        Geometry.IsInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    }
    */

    private Triangle[] Triangulate()
    {
        Vector2 centroid = Vector2.zero;
        foreach (Vector2 vert in verts)
        {
            centroid += vert;
        }
        centroid /= verts.Length;
        float r = 0.0f;
        foreach (Vector2 vert in verts)
        {
            r = Mathf.Max(r, Vector2.Distance(vert, centroid));
        }
        //Add some padding to avoid floating point errors later on
        r *= 1.2f;

        Vector2 pi0 = new Vector2(centroid.x, centroid.y - 2 * r);
        Vector2 pi1 = new Vector2(centroid.x + r * Mathf.Sqrt(3), centroid.y + r);
        Vector2 pi2 = new Vector2(centroid.x - r * Mathf.Sqrt(3), centroid.y + r);

        Debug.DrawLine(pi0, pi1, Color.cyan, 5.0f, false);
        Debug.DrawLine(pi1, pi2, Color.cyan, 5.0f, false);
        Debug.DrawLine(pi2, pi0, Color.cyan, 5.0f, false);

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
            Debug.Log(i);
            Debug.Log(v.p);
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

            HalfEdge e30Bound = new HalfEdge(e30.origin);
            HalfEdge e31Bound = new HalfEdge(e31.origin);
            HalfEdge e32Bound = new HalfEdge(e32.origin);
            e30Bound.incidentTriangle = tri0;
            e31Bound.incidentTriangle = tri1;
            e32Bound.incidentTriangle = tri2;

            HalfEdge e03Bound = new HalfEdge(e03.origin);
            HalfEdge e13Bound = new HalfEdge(e13.origin);
            HalfEdge e23Bound = new HalfEdge(e23.origin);
            e03Bound.incidentTriangle = tri2;
            e13Bound.incidentTriangle = tri0;
            e23Bound.incidentTriangle = tri1;

            HalfEdge.SetTwins(e30Bound, e03Bound);
            HalfEdge.SetTwins(e31Bound, e13Bound);
            HalfEdge.SetTwins(e32Bound, e23Bound);
            containingTri.childrenBounds = new List<HalfEdge> { e30Bound, e31Bound, e32Bound };

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
                t += 1;
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

                        HalfEdge dcBound = new HalfEdge(dc.origin);
                        dcBound.incidentTriangle = adc;
                        HalfEdge cdBound = new HalfEdge(cd.origin);
                        cdBound.incidentTriangle = bcd;
                        HalfEdge.SetTwins(dcBound, cdBound);
                        ab.incidentTriangle.childrenBounds = new List<HalfEdge> { dcBound };
                        ba.incidentTriangle.childrenBounds = new List<HalfEdge> { dcBound };

                        frontier.AddLast(ad);
                        frontier.AddLast(db);
                    }
                    sptSet.Add(ab);
                    sptSet.Add(ba);
                }
            }
            //Debug.Log(t);
        }

        //Generate Triangle list
        List<Triangle> leafs = new List<Triangle>();
        HashSet<Triangle> visited = new HashSet<Triangle>();

        int count = 0;
        treeRoot.GetRealLeafs(ref leafs, ref visited, ref count, 0);
        Debug.Log(count);
        
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

    
}
