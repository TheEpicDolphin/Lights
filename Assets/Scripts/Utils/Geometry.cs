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

        internal static bool IntersectRays2D(Vector2 p1, Vector2 dir1, Vector2 p2, Vector2 dir2, out Vector2 intersection)
        {
            float det = Det(dir2, dir1);

            //Lines are parallel
            if (Mathf.Abs(det) < 1e-5)
            {
                intersection = Vector2.zero;
                return false;
            }

            float t = Det(p1 - p2, dir2) / det;
            intersection = p1 + t * dir1;
            return true;
        }

        internal static bool IntersectLines2D(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2, out Vector2 intersection)
        {
            Vector2 dir1 = (e1 - s1).normalized;
            Vector2 dir2 = (e2 - s2).normalized;
            float det = Det(dir2, dir1);

            //Lines are parallel
            if (Mathf.Abs(det) < 1e-5)
            {
                intersection = Vector2.zero;
                return false;
            }

            float t = Det(s1 - s2, dir2) / det;
            intersection = s1 + t * dir1;
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

        internal static bool IsInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            //barycentric coordinates: p = a + u * (c - a) + v * (b - a)
            Vector2 v0 = c - a;
            Vector2 v1 = b - a;
            Vector2 v2 = p - a;

            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float inv_denom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * inv_denom;
            float v = (dot00 * dot12 - dot01 * dot02) * inv_denom;
            float w = 1 - u - v;
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        internal static Vector3 ToBarycentricCoordinates(Vector2[] triPoints, Vector2 p)
        {
            return ToBarycentricCoordinates(triPoints[0], triPoints[1], triPoints[2], p);
        }

        internal static Vector3 ToBarycentricCoordinates(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            //barycentric coordinates: p = a + u * (c - a) + v * (b - a)
            Vector2 v0 = c - a;
            Vector2 v1 = b - a;
            Vector2 v2 = p - a;

            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float inv_denom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * inv_denom;
            float v = (dot00 * dot12 - dot01 * dot02) * inv_denom;
            float w = 1 - u - v;
            return new Vector3(u, v, w);
        }

        //Vertices a, b, and c are ordered counterclockwise
        internal static bool IsInCircumscribedCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            Vector2 m1 = (a + b) / 2;
            Vector2 m2 = (b + c) / 2;
            Vector2 dir1 = Vector2.Perpendicular(b - a).normalized;
            Vector2 dir2 = Vector2.Perpendicular(c - b).normalized;
            Vector2 circumCenter;
            if(IntersectRays2D(m1, dir1, m2, dir2, out circumCenter))
            {
                if ((p - circumCenter).magnitude < (a - circumCenter).magnitude)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /*  trans must be in the format: (similar to transform.localToWorldMatrix)
         *  [ right.x    up.x    forward.x   pos.x ]
         *  [ right.y    up.y    forward.y   pos.y ]
         *  [ right.z    up.z    forward.z   pos.z ]
         *  [   0         0         0         1    ]
         *  
         *  n is normal of plane. p0 is any arbitrary point on the plane
         *  
         *  return transform that is reflected across the plane
         */
        internal static Matrix4x4 ReflectionTransformAcrossPlane(Vector3 n, Vector3 p0)
        {
            Matrix4x4 trans_p0 = Matrix4x4.Translate(-p0);
            Matrix4x4 rotToNegXAxis = Matrix4x4.Rotate(Quaternion.FromToRotation(n, new Vector3(-1, 0, 0)));
            Matrix4x4 reflectYAxis = Matrix4x4.identity;
            reflectYAxis.SetColumn(0, new Vector4(-1, 0, 0, 0));

            Matrix4x4 M = trans_p0.inverse * rotToNegXAxis.inverse * reflectYAxis * rotToNegXAxis * trans_p0;
            //Maybe invert this for correctness?
            return M;
        }

        internal static Matrix4x4 RefractionTransformWithPlane(Vector3 n, Vector3 p0, Vector3 vI, float n2_n1)
        {
            float dot_vI_n = Vector3.Dot(vI, n);
            //Refracted direction
            Vector3 vR = (vI - dot_vI_n * n) / n2_n1 +
                        Mathf.Sqrt(1 - (1 - dot_vI_n * dot_vI_n) / (n2_n1 * n2_n1)) * n;
            vR.Normalize();
            float cos_theta1 = dot_vI_n;
            float cos_theta2 = Vector3.Dot(vR, n);

            //Prevent division by 0 errors
            if(Mathf.Approximately(cos_theta1, 0.0f))
            {
                return Matrix4x4.identity;
            }

            //If theta2 == pi/2, we have reached the critical angle.
            if(Mathf.Approximately(cos_theta2, 0.0f))
            {
                //Total internal reflection only
                return ReflectionTransformAcrossPlane(n, p0);
            }
            else
            {
                //Refraction
                Matrix4x4 rotToRefrDir = Matrix4x4.Rotate(Quaternion.FromToRotation(vI, vR));
                float d = Vector3.Dot(p0, n);
                float dRefr = n2_n1 * (cos_theta2 / cos_theta1) * d;
                Matrix4x4 translate_depth_diff = Matrix4x4.Translate(-(dRefr - d) * n);

                Matrix4x4 M = translate_depth_diff * rotToRefrDir;
                return M.inverse;
            }
            
        }

        /*
        internal static Matrix4x4 RefractTransformWithPlane(Vector3 n, Vector3 p0, Vector3 vI, float n2_n1)
        {
            float dot_vI_n = Vector3.Dot(vI, n);
            Vector3 vR = (vI - dot_vI_n * n) / n2_n1 + 
                        Mathf.Sqrt(1 - (1 - dot_vI_n * dot_vI_n) /(n2_n1 * n2_n1)) * n;
            vR.Normalize();

            float cos_theta1 = dot_vI_n;
            float cos_theta2 = Vector3.Dot(vR, n);

            Matrix4x4 trans_p0 = Matrix4x4.Translate(-p0);
            Vector3 nTrans = new Vector3(0, -1, 0);
            //Vector3 nTrans = new Vector3(-1, 0, 0);
            Matrix4x4 rotToNegYAxis = Matrix4x4.Rotate(Quaternion.FromToRotation(n, nTrans));

            Matrix4x4 refractXaxis = Matrix4x4.identity;
            refractXaxis.SetColumn(1, new Vector4(0, n2_n1 * cos_theta2 / cos_theta1, 0, 0));

            Matrix4x4 M = trans_p0.inverse * rotToNegYAxis.inverse * refractXaxis * rotToNegYAxis * trans_p0;
            return M;
        }
        */
    }
}
    
