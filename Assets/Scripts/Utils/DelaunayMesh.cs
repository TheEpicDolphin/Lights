﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;
using AlgorithmUtils;
using VecUtils;

//Traverse edges counterclockwise
public class Triangle
{
    public HalfEdge edge;
    public List<Triangle> children;
    private Vertex[] ghostBounds;

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
        this.ghostBounds = new Vertex[] { e01.origin, e12.origin, e20.origin };
    }

    //Possibly return an edge rather than a triangle if point lies between to triangles
    public Triangle FindContainingTriangle(Vector2 p)
    {
        if (children.Count == 0)
        {
            //Possibly check if point lies on edge
            return this;
        }
        else
        {
            float maxMin = Mathf.NegativeInfinity;
            Triangle closestChild = children[0];
            foreach (Triangle child in children)
            {
                Vector3 uvw = Geometry.ToBarycentricCoordinates(child.ghostBounds[0].p, 
                                                                child.ghostBounds[1].p, 
                                                                child.ghostBounds[2].p, 
                                                                p);

                if (uvw[0] >= 0 && uvw[1] >= 0 && uvw[2] >= 0)
                {
                    return child.FindContainingTriangle(p);
                }
                else
                {
                    float minD = Mathf.Min(uvw[0], Mathf.Min(uvw[1], uvw[2]));
                    if(minD > maxMin)
                    {
                        maxMin = minD;
                        closestChild = child;
                    }
                }
            }
            return closestChild.FindContainingTriangle(p);
            
        }

    }

    private bool Contains(Vector2 p)
    {
        return Geometry.IsInTriangle(ghostBounds[0].p, ghostBounds[1].p, ghostBounds[2].p, p);
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
        if (this.children.Count == 0)
        {
            Debug.Log("Depth: " + depth.ToString());
            if (!this.IsImaginary())
            {
                leafs.Add(this);
            }
        }
        else
        {
            foreach(Triangle child in children)
            {
                child.GetRealLeafs(ref leafs, ref visited, ref count, depth + 1);
            }
        }

        visited.Add(this);
    }

    public HalfEdge[] InsertVertex(Vertex v)
    {
        HalfEdge e01 = this.edge;
        HalfEdge e12 = e01.next;
        HalfEdge e20 = e12.next;

        this.edge = null;

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

        v.AddOutgoingEdge(e30);
        v.AddOutgoingEdge(e31);
        v.AddOutgoingEdge(e32);

        this.children = new List<Triangle>() { tri0, tri1, tri2 };
        return new HalfEdge[] { e01, e12, e20 };
    }

    public HalfEdge[] LawsonFlip(HalfEdge ab)
    {
        HalfEdge ba = ab.twin;

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

            //Remove old edges from vertices
            ab.origin.RemoveOutgoingEdge(ab);
            ba.origin.RemoveOutgoingEdge(ba);
            //Add new edges from flip to vertices 
            cd.origin.AddOutgoingEdge(cd);
            dc.origin.AddOutgoingEdge(dc);

            ab.incidentTriangle.children = new List<Triangle> { adc, bcd };
            ba.incidentTriangle.children = new List<Triangle> { adc, bcd };

            frontier.AddLast(ad);
            frontier.AddLast(db);
            return new HalfEdge[] { ad, db };
        }

        return new HalfEdge[] { };
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
    //Sorted counter-clockwise
    private LinkedList<HalfEdge> outgoingEdges;

    public Vertex(Vector2 p, int i)
    {
        this.p = p;
        this.i = i;
        outgoingEdges = new LinkedList<HalfEdge>();
    }

    public HalfEdge GetOutgoingEdgeClockwiseFrom(Vector2 dir)
    {
        LinkedListNode<HalfEdge> curEdgeNode = outgoingEdges.First;
        while (curEdgeNode.Next != null)
        {
            HalfEdge outgoingEdge = curEdgeNode.Value;
            HalfEdge nextOutgoingEdge = curEdgeNode.Next.Value;
            Vector2 dirCur = outgoingEdge.next.origin.p - outgoingEdge.origin.p;
            Vector2 dirNext = nextOutgoingEdge.next.origin.p - nextOutgoingEdge.origin.p;
            if ((VecMath.Det(dirCur, dir) > 0) && (VecMath.Det(dir, dirNext) > 0))
            {
                return curEdgeNode.Value;
            }
            curEdgeNode = curEdgeNode.Next;
        }
        return outgoingEdges.Last.Value;
    }

    public void AddOutgoingEdge(HalfEdge e)
    {
        Vector2 newDir = e.next.origin.p - e.origin.p;
        LinkedListNode<HalfEdge> curEdgeNode = outgoingEdges.First;
        while (curEdgeNode.Next != null)
        {
            HalfEdge outgoingEdge = curEdgeNode.Value;
            HalfEdge nextOutgoingEdge = curEdgeNode.Next.Value;
            Vector2 dirCur = outgoingEdge.next.origin.p - outgoingEdge.origin.p;
            Vector2 dirNext = nextOutgoingEdge.next.origin.p - nextOutgoingEdge.origin.p;
            if ((VecMath.Det(dirCur, newDir) > 0) && (VecMath.Det(newDir, dirNext) > 0))
            {
                outgoingEdges.AddAfter(curEdgeNode, e);
                return;
            }
            curEdgeNode = curEdgeNode.Next;
        }
        outgoingEdges.AddLast(e);
    }

    public void RemoveOutgoingEdge(HalfEdge e)
    {
        outgoingEdges.Remove(e);
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
            //Debug.Log(i);
            //Debug.Log(v.p);
            Triangle containingTri = treeRoot.FindContainingTriangle(v.p);
            HalfEdge[] edges = containingTri.InsertVertex(v);

            //Flip triangles that don't satisfy delaunay property
            HashSet<HalfEdge> sptSet = new HashSet<HalfEdge>();
            LinkedList<HalfEdge> frontier = new LinkedList<HalfEdge>();
            frontier.AddLast(edges[0]);
            frontier.AddLast(edges[1]);
            frontier.AddLast(edges[2]);

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

                        ab.incidentTriangle.children = new List<Triangle> { adc, bcd };
                        ba.incidentTriangle.children = new List<Triangle> { adc, bcd };

                        frontier.AddLast(ad);
                        frontier.AddLast(db);
                    }
                    sptSet.Add(ab);
                    sptSet.Add(ba);
                }
            }
        }

        //Insert constrained edges


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


