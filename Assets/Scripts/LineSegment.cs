using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VecUtils;

public class LineSegment
{
    public Vector2 p1 = Vector2.zero;
    public Vector2 p2 = Vector2.zero;
    public Vector2 dir
    {
        get {
            return (p2 - p1).normalized;
        }
    }
    public LineSegment(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    //Intersect line segment with line
    public bool Intersect(Vector2 p, Vector2 dir, ref Vector2 intersectionPoint, bool isBounded)
    {
        Vector2 n1 = -Vector2.Perpendicular(this.dir);
        Vector2 n2 = -Vector2.Perpendicular(dir);
        float determinant = VecMath.Det(n1, n2);
        if (Mathf.Abs(determinant) > Mathf.Epsilon)
        {
            float Dx = Vector2.Dot(n1, this.p1) * n2.y - n1.y * Vector2.Dot(n2, p);
            float Dy = n1.x * Vector2.Dot(n2, p) - Vector2.Dot(n1, this.p1) * n2.x;
            intersectionPoint = new Vector2(Dx / determinant, Dy / determinant);

            if (Vector2.Dot(intersectionPoint - this.p1, intersectionPoint - this.p2) < 0)
            {
                if (isBounded)
                {
                    return Vector2.Dot(intersectionPoint - p, dir) > 0;
                }
                return true;
            }

        }
        return false;
    }

    //Intersection between line segments
    public static bool Intersection(LineSegment l1, LineSegment l2, ref Vector2 intersectionPoint)
    {
        if(l1.Intersect(l2.p1, l2.dir, ref intersectionPoint, false))
        {
            if(Vector2.Dot(intersectionPoint - l2.p1, intersectionPoint - l2.p2) < 0)
            {
                return true;
            }
        }
        return false;
    }

    
    public string ToString(string format)
    {
        return "(" + p1.ToString(format) + ", " + p2.ToString(format) + ")";
    }
    

    
}
