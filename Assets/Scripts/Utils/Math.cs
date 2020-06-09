﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MathUtils
{
    /**
     * <summary>Contains functions and constants used in multiple classes.
     * </summary>
     */
    public struct Math
    {
        internal static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        internal static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }


    }

    public struct PolarCoord
    {
        public float r;
        public float theta;
        public PolarCoord(float r, float theta)
        {
            this.r = r;
            this.theta = Math.Mod(theta, 2 * Mathf.PI);
        }

        public static PolarCoord ToPolarCoords(Vector2 p)
        {
            float ccangle = Mathf.Atan2(p.y, p.x);
            if (ccangle < 0)
            {
                ccangle += 2 * Mathf.PI;
            }
            return new PolarCoord(p.magnitude, ccangle);
        }

        public float x()
        {
            return r * Mathf.Cos(theta);
        }

        public float y()
        {
            return r * Mathf.Sin(theta);
        }

        public static float Interpolate(PolarCoord p1, PolarCoord p2, float t)
        {
            float y1 = p1.y();
            float y2 = p2.y();
            float x1 = p1.x();
            float x2 = p2.x();

            float theta0;
            if(x2 == x1)
            {
                theta0 = Mathf.PI / 2;
            }
            else
            {
                theta0 = Mathf.Atan((y2 - y1) / (x2 - x1));
            }

            float d0 = p1.r * Mathf.Sin(p1.theta - theta0);
            return d0 / Mathf.Sin(t - theta0);
        }
    }
}
