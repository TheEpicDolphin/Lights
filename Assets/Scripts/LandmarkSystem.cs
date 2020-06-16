using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmarkSystem
{
    Dictionary<Vector2Int, Landmark> spatialHashedGrid;
    float s;

    public LandmarkSystem(float s)
    {
        spatialHashedGrid = new Dictionary<Vector2Int, Landmark>();
        this.s = s;
    }

    public void AddLandmarksAroundObstacle(Obstacle obstacle)
    {
        float r = s * Mathf.Sqrt(2);
        Vector2[] outline = obstacle.GetWorldMinkowskiBoundVerts(r, clockwise: false);

        List<Landmark> active = new List<Landmark>();
        
        for(int i = 0; i < outline.Length; i++)
        {
            Vector2 p0 = outline[i];
            Vector2 p1 = outline[(i + 1) % outline.Length];
            float T = (p1 - p0).magnitude;

            float t = r / 2;
            while (t / T < 1)
            {
                Vector2 p = Vector2.Lerp(p0, p1, t / T);
                Vector2Int gridCoords = ToGridCoordinates(p);
                
                if (!spatialHashedGrid.ContainsKey(gridCoords))
                {
                    Landmark lm = new Landmark(p);
                    spatialHashedGrid[gridCoords] = lm;
                    active.Add(lm);
                }
                t += r;
            }
        }

        //Using Poisson Disk sampling, insert more landmarks around the obstacle
        int k = 20;
        while(active.Count > 0)
        {
            int ri = Random.Range(0, active.Count);
            Landmark lm = active[ri];
            bool found = false;
            for(int tries = 0; tries < k; tries++)
            {
                /* Pick a random angle */
                float theta = Random.Range(0, 2 * Mathf.PI);
                /* Pick a random radius between r and 2r */
                float newRadius = Random.Range(r, 2 * r);
                /* Find X & Y coordinates relative to point p */
                float newX = lm.p.x + newRadius * Mathf.Cos(theta);
                float newY = lm.p.y + newRadius * Mathf.Sin(theta);
                Vector2 pNew = new Vector2(newX, newY);
                
                //Check if new point is valid
            }

            if (!found)
            {
                active.RemoveAt(ri);
            }

        }

    }

    Vector2Int ToGridCoordinates(Vector2 p)
    {
        int xi = Mathf.FloorToInt(p.x / s);
        int yi = Mathf.FloorToInt(p.y / s);
        return new Vector2Int(xi, yi);
    }

    public List<Landmark> GetLandmarksWithinRadius(float radius)
    {

    }

}
