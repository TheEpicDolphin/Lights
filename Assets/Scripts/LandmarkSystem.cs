using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmarkSystem
{
    NavigationMesh navMesh;
    Dictionary<Vector2Int, Landmark> spatialHashedGrid;
    float s;

    public LandmarkSystem(float s, NavigationMesh navMesh)
    {
        spatialHashedGrid = new Dictionary<Vector2Int, Landmark>();
        this.s = s;
        this.navMesh = navMesh;
    }

    public void AddLandmarksAroundObstacle(Obstacle obstacle)
    {
        float d = s * Mathf.Sqrt(2);
        float r = d / 2;
        Vector2[] outline = obstacle.GetWorldMinkowskiBoundVerts(r, clockwise: false);

        List<Landmark> active = new List<Landmark>();
        
        for(int i = 0; i < outline.Length; i++)
        {
            Vector2 p0 = outline[i];
            Vector2 p1 = outline[(i + 1) % outline.Length];
            float T = (p1 - p0).magnitude;

            float t = r;
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
                t += 2 * r;
            }
        }

        /* Using Poisson Disk sampling, insert more landmarks around the obstacle */
        int k = 30;
        while(active.Count > 0)
        {
            int ri = Random.Range(0, active.Count);
            Landmark lm = active[ri];
            bool found = false;
            for(int tries = 0; tries < k; tries++)
            {
                /* Pick a random angle */
                float theta = Random.Range(0, 2 * Mathf.PI);
                /* Pick a random separation between d and 2d */
                float newSep = Random.Range(d, 2 * d);
                /* Find X & Y coordinates relative to point p */
                float newX = lm.p.x + newSep * Mathf.Cos(theta);
                float newY = lm.p.y + newSep * Mathf.Sin(theta);
                Vector2 pNew = new Vector2(newX, newY);
                
                /* Check if new point is roughly valid */
                if(IsPointRoughlyValid(pNew, d))
                {
                    Landmark newL = new Landmark(pNew);
                    Vector2Int gridCoords = ToGridCoordinates(pNew);
                    spatialHashedGrid[gridCoords] = newL;
                    active.Add(newL);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                active.RemoveAt(ri);
            }

        }

    }

    bool IsPointRoughlyValid(Vector2 p, float d)
    {
        if (!navMesh.IsLocationValid(p))
        {
            return false;
        }

        Vector2Int gridCoords = ToGridCoordinates(p);
        int i0 = gridCoords.x - 1;
        int i1 = gridCoords.x + 1;
        int j0 = gridCoords.y - 1;
        int j1 = gridCoords.y + 1;

        for(int i = i0; i <= i1; i++)
        {
            for(int j = j0; j <= j1; j++)
            {
                Vector2Int neighborGridCoords = new Vector2Int(i, j);
                if (spatialHashedGrid.ContainsKey(neighborGridCoords))
                {
                    Landmark neighbor = spatialHashedGrid[neighborGridCoords];
                    if (Vector2.Distance(p, neighbor.p) < d)
                    {
                        return false;
                    }
                }
                
            }
        }

        return true;
    }

    Vector2Int ToGridCoordinates(Vector2 p)
    {
        int xi = Mathf.FloorToInt(p.x / s);
        int yi = Mathf.FloorToInt(p.y / s);
        return new Vector2Int(xi, yi);
    }

    public List<Landmark> GetLandmarksWithinRadius(Vector2 p, float radius)
    {
        Vector2Int gridCoords = ToGridCoordinates(p);
        float k = radius / this.s;

        

        List<Landmark> nearbyLandmarks = new List<Landmark>();

        for (int i = i0; i <= i1; i++)
        {
            for (int j = j0; j <= j1; j++)
            {
                Vector2Int neighborGridCoords = new Vector2Int(i, j);
                if (spatialHashedGrid.ContainsKey(neighborGridCoords))
                {
                    Landmark neighbor = spatialHashedGrid[neighborGridCoords];
                    nearbyLandmarks.Add(neighbor);
                }

            }
        }
    }

    public void DrawLandmarks()
    {
        
    }

}
