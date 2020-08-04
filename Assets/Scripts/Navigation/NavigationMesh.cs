using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VecUtils;
using System.Linq;

//Assumes navmesh is perpendicular to z axis
public class NavigationMesh : MonoBehaviour
{
    public List<Obstacle> obstacles;
    Graph<Triangle> navMeshGraph;
    DelaunayMesh mesh;
    Material mat;
    public float aiRadius = 0.6f;

    private void Awake()
    {
        mat = GetComponent<Renderer>().material;

        List<Vector2> verts = new List<Vector2>();
        int C = 4;
        int R = 4;
        Vector2 xBounds = new Vector2(-10.0f, 10.0f);
        Vector2 yBounds = new Vector2(-10.0f, 10.0f);

        float W = (xBounds[1] - xBounds[0]);
        float H = (yBounds[1] - yBounds[0]);

        for(int i = 0; i < R + 1; i++)
        {
            float y = yBounds[0] + i * H / R;
            for(int j = 0; j < C + 1; j++)
            {
                float x = xBounds[0] + j * W / C;
                verts.Add(new Vector2(x, y));
            }
        }

        List<Vector2[]> constrainedPoints = new List<Vector2[]>();
        foreach (Obstacle obstacle in obstacles)
        {
            obstacle.InitializeEdges();
            constrainedPoints.Add(obstacle.GetWorldMinkowskiBoundVerts(aiRadius, true));
        }

        mesh = new DelaunayMesh(verts.ToArray(), constrainedPoints);
        navMeshGraph = new Graph<Triangle>(mesh.tris);

        float s = 2 * aiRadius / Mathf.Sqrt(2);
    }

    List<Vector2> CentroidPath(List<Triangle> triPath, Vector2 startPos, Vector2 targetPos)
    {
        List<Vector2> path = new List<Vector2>();
        foreach (Triangle tri in triPath)
        {
            Vector2 c = (tri.edge.prev.origin.p + tri.edge.origin.p + tri.edge.next.origin.p) / 3;
            path.Add(c);
        }
        path.Add(targetPos);
        return path;
    }

    List<Vector2> StringPullingAlgorithm(List<Triangle> triPath, Vector2 startPos, Vector2 targetPos)
    {
        if (triPath.Count == 1)
        {
            return new List<Vector2> { startPos, targetPos };
        }

        List<Vector2> edgePortals = new List<Vector2>();
        for(int i = 0; i < triPath.Count - 1; i++)
        {
            Triangle tri = triPath[i];
            List<INodeEdge> neighborEdges = tri.GetNeighborEdges();
            foreach (INodeEdge neighborEdge in neighborEdges)
            {
                Triangle nextTri = (Triangle) neighborEdge.GetNode();
                if (nextTri == triPath[i + 1])
                {
                    HalfEdge portal = ((HalfEdge) neighborEdge);
                    edgePortals.Add(portal.origin.p);
                    edgePortals.Add(portal.next.origin.p);
                    break;
                }
            }
        }

        edgePortals.Add(targetPos);
        edgePortals.Add(targetPos);

        /*
        for(int i = 0; i < edgePortals.Count; i+=2)
        {
            //Debug.DrawLine(startPos, transform.TransformPoint(edgePortals[i]), Color.cyan, 0.0f, false);
            Debug.DrawLine(transform.TransformPoint(edgePortals[i]),
                transform.TransformPoint(edgePortals[i + 1]), Color.magenta, 0.0f, false);
        }
        */
        
        //Run Simple Stupid Funnel Algorithm.
        List<Vector2> breadCrumbs = new List<Vector2> { startPos };
        Vector2 funnelL = edgePortals[0] - breadCrumbs[breadCrumbs.Count - 1];
        Vector2 funnelR = edgePortals[1] - breadCrumbs[breadCrumbs.Count - 1];

        
        int epL = 2;
        int epR = 2;
        int ep = 2;
        int t = 0;
        while (ep < edgePortals.Count && t < 100)
        {
            t += 1;
            Vector2 newFunnelL = edgePortals[ep] - breadCrumbs[breadCrumbs.Count - 1];
            if (VecMath.Det(newFunnelL, funnelL) >= 0)
            {
                /* The new left funnel is more clockwise than current left funnel */
                if (edgePortals[ep] == edgePortals[epL] || VecMath.Det(funnelR, newFunnelL) >= 0)
                {
                    /* New left funnel does NOT cross over right funnel */
                    funnelL = newFunnelL;
                    epL = ep;
                }
                else
                {
                    breadCrumbs.Add(edgePortals[epR + 1]);
                    ep = epR + 2;
                    epL = ep;
                    epR = ep;
                    funnelL = edgePortals[epL] - breadCrumbs[breadCrumbs.Count - 1];
                    funnelR = edgePortals[epR + 1] - breadCrumbs[breadCrumbs.Count - 1];
                    continue;
                }

            }

            Vector2 newFunnelR = edgePortals[ep + 1] - breadCrumbs[breadCrumbs.Count - 1];
            if (VecMath.Det(funnelR, newFunnelR) >= 0)
            {
                /* The new right funnel is more counter-clockwise than current right funnel */
                if (edgePortals[ep + 1] == edgePortals[epR + 1] || VecMath.Det(newFunnelR, funnelL) >= 0)
                {
                    /* New right funnel does NOT cross over left funnel */
                    funnelR = newFunnelR;
                    epR = ep;
                }
                else
                {
                    breadCrumbs.Add(edgePortals[epL]);
                    ep = epL + 2;
                    epL = ep;
                    epR = ep;
                    funnelL = edgePortals[epL] - breadCrumbs[breadCrumbs.Count - 1];
                    funnelR = edgePortals[epR + 1] - breadCrumbs[breadCrumbs.Count - 1];
                    continue;
                }
            }
            ep += 2;
        }

        if (t == 100)
        {
            Debug.LogError("FUNNEL ALG FAILED");
        }

        breadCrumbs.Add(targetPos);
        return breadCrumbs;

    }

    public Vector2[] GetShortestPathFromTo(Vector2 start, Vector2 destination)
    {
        Triangle startTri = FindContainingTriangle(start);
        Triangle endTri = FindContainingTriangle(destination);

        List<Triangle> triPath = navMeshGraph.DijkstrasAlgorithm(startTri, endTri);
        List<Vector2> shortestPath = StringPullingAlgorithm(triPath, start, destination);
        return shortestPath.ToArray();
    }

    public List<Vector2[]> GetShortestPathsFromTo(Vector2 start, List<Vector2> destinations)
    {
        Triangle startTri = FindContainingTriangle(start);
        List<Triangle> endTris = new List<Triangle>();
        foreach (Vector2 destination in destinations)
        {
            endTris.Add(FindContainingTriangle(destination));
        }
        List<List<Triangle>> triPaths = navMeshGraph.DijkstrasAlgorithm(startTri, endTris);
        List<Vector2[]> shortestPaths = new List<Vector2[]>();
        for (int i = 0; i < triPaths.Count; i++)
        {
            List<Vector2> shortestPath = StringPullingAlgorithm(triPaths[i], start, destinations[i]);
            shortestPaths.Add(shortestPath.ToArray());
        }
        return shortestPaths;
    }

    private Triangle FindContainingTriangle(Vector2 p)
    {
        return mesh.FindContainingTriangle(p);
    }

    public bool IsLocationValid(Vector2 p)
    {
        Triangle tri = FindContainingTriangle(p);
        return !tri.isIntersectingHole;
    }

    public void Draw()
    {
        if (!mat)
        {
            Debug.LogError("Material not assigned");
            return;
        }
        
        GL.PushMatrix();
        mat.SetPass(0);

        //GL.LoadOrtho();
        
        //GL.LoadIdentity();
        //GL.MultMatrix();

        GL.Begin(GL.LINES);
        mesh.Draw();
        GL.End();

        GL.PopMatrix();
    }

}


