using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Use submeshes and multiple materials for beams in room

//Oriented so that when walking along the direction of the line segments, the inside of the polygon is to the left
//Convex
public class Polygon
{
    public List<Vector2> verts;

    public Polygon(List<Vector2> verts)
    {
        this.verts = verts;
    }

    public static bool Intersection(Polygon pgon1, Polygon pgon2, ref Polygon intersection)
    {
        List<Vector2> intersectionVerts = new List<Vector2>();

        //Find first instance when we enter inside pgon2
        int i = 1;
        while (i < pgon1.verts.Count + 1)
        {
            LineSegment l1 = new LineSegment(pgon1.verts[i - 1], pgon1.verts[i % pgon1.verts.Count]);
            float d0 = Vector2.Dot(l1.p1, l1.dir);
            float d1 = Vector2.Dot(l1.p2, l1.dir);
            float tMin = Mathf.Infinity;
            int jMin = -1;

            for (int j = 1; j < pgon2.verts.Count + 1; j++)
            {
                LineSegment l2 = new LineSegment(pgon2.verts[j - 1], pgon2.verts[j % pgon2.verts.Count]);
                Vector2 intersectionPoint = Vector2.zero;
                if (LineSegment.Intersection(l1, l2, ref intersectionPoint))
                {
                    float t = Vector2.Dot(intersectionPoint, l1.dir) - d0;
                    if (t < tMin)
                    {
                        jMin = j;
                        tMin = t;
                    }
                }
            }


            if (tMin < d1 - d0)
            {
                LineSegment l2 = new LineSegment(pgon2.verts[jMin - 1], pgon2.verts[jMin % pgon2.verts.Count]);
                Vector2 n2 = Vector2.Perpendicular(l2.dir);
                //Check if pgon1's edge is going inside of pgon2
                if (Vector2.Dot(l1.dir, n2) > 0)
                {
                    intersectionVerts.Add(l1.p1 + tMin * l1.dir);
                    break;
                }
            }
            i += 1;
        }

        

        if (intersectionVerts.Count == 0)
        {
            Debug.Log("NO INTERSECTION");
            //Use horizontal line test to determine whether point is inside or outside polygon
            int intersections = 0;
            for(int j = 0; j < pgon2.verts.Count + 1; j++)
            {
                LineSegment e = new LineSegment(pgon2.verts[j - 1], pgon2.verts[j % pgon2.verts.Count]);
                Vector2 intersectionPoint = Vector2.zero;
                if (e.Intersect(pgon1.verts[0], Vector2.right, ref intersectionPoint, true))
                {
                    intersections += 1;
                }
            }

            //pgon1 is outside pgon2
            if (intersections % 2 == 0)
            {
                return false;
            }
            //pgon1 is inside pgon2
            else
            {

            }
            return false;
        }

        Polygon innerPgon = pgon1;
        Polygon outerPgon = pgon2;
        while (intersectionVerts.Count < pgon1.verts.Count + pgon2.verts.Count)
        {
            Vector2 dir = (innerPgon.verts[i % innerPgon.verts.Count] - innerPgon.verts[i > 0 ? i - 1 : innerPgon.verts.Count - 1]).normalized;
            Vector2 startRef = intersectionVerts[intersectionVerts.Count - 1] + Vector2.kEpsilon * dir;
            LineSegment l1 = new LineSegment(startRef, innerPgon.verts[i % innerPgon.verts.Count]);
            Debug.Log(l1.ToString("F4"));
            float d0 = Vector2.Dot(l1.p1, l1.dir);
            float d1 = Vector2.Dot(l1.p2, l1.dir);

            float tMin = Mathf.Infinity;
            int jMin = -1;

            for (int j = 1; j < outerPgon.verts.Count + 1; j++)
            {
                LineSegment l2 = new LineSegment(outerPgon.verts[j - 1], outerPgon.verts[j % outerPgon.verts.Count]);
                Vector2 intersectionPoint = Vector2.zero;
                if (LineSegment.Intersection(l1, l2, ref intersectionPoint))
                {
                    float t = Vector2.Dot(intersectionPoint, l1.dir) - d0;
                    if (t < tMin)
                    {
                        jMin = j % outerPgon.verts.Count;
                        tMin = t;
                    }
                }
            }

            if (tMin < d1 - d0)
            {
                Vector2 closestIntersectionPoint = l1.p1 + tMin * l1.dir;
                
                if(Vector2.SqrMagnitude(intersectionVerts[0] - closestIntersectionPoint) < Vector2.kEpsilon)
                {
                    break;
                }
                //innerPgon is exiting outerPgon
                intersectionVerts.Add(closestIntersectionPoint);
                
                Polygon temp = innerPgon;
                innerPgon = outerPgon;
                outerPgon = temp;
                i = jMin;
            }
            else
            {
                intersectionVerts.Add(l1.p2);
                i = (i + 1) % innerPgon.verts.Count;
            }

        }
        intersection = new Polygon(intersectionVerts);
        return true;
    }

    public void Draw(Color color, float duration, float yOffset = 0.0f)
    {
        for(int i = 1; i < verts.Count + 1; i++)
        {
            Vector3 v1 = new Vector3(verts[i - 1].x, yOffset, verts[i - 1].y);
            Vector3 v2 = new Vector3(verts[i % verts.Count].x, yOffset, verts[i % verts.Count].y);
            Debug.DrawLine(v1, v2, color, duration);
        }
    }

}
