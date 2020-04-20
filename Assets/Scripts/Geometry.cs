using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryUtils
{
    public class Geometry
    {
        internal static float Det(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        internal static bool IntersectLines2D(Vector2 p1, Vector2 dir1, Vector2 p2, Vector2 dir2, out float t)
        {
            float det = Det(dir2, dir1);

            //Lines are parallel
            if (det < 1e-5)
            {
                t = 0.0f;
                return false;
            }

            t = Det(p1 - p2, dir2) / det;
            return true;
        }

        internal static bool IsInPolygon(Vector2 v, Vector2[] p, bool counterClockwise = true)
        {
            if (counterClockwise)
            {
                System.Array.Reverse(p);
            }

            //Points are clockwise
            int j = p.Length - 1;
            bool c = false;
            for (int i = 0; i < p.Length; j = i++)
            {
                c ^= p[i].y > v.y ^ p[j].y > v.y && v.x < (p[j].x - p[i].x) * (v.y - p[i].y) / (p[j].y - p[i].y) + p[i].x;
            }
            return c;

        }
    }
}
    
