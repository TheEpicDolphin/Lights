using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GeometryUtils;
using VecUtils;


public interface INode
{
    List<INodeEdge> GetNeighborEdges();
}

public interface INodeEdge
{
    INode GetNode();
    int GetWeight();
}

public class Graph<T> where T : class, INode
{
    public T[] nodes;

    public Graph(T[] nodes)
    {
        this.nodes = nodes;
    }

    public List<T> DijkstrasAlgorithm(T start, T end)
    {
        HashSet<T> sptSet = new HashSet<T>();
        Dictionary<T, T> backPointers = new Dictionary<T, T>();

        MinHeap<int, T> frontier = new MinHeap<int, T>();
        foreach (T node in nodes)
        {
            if (node == start)
            {
                frontier.Insert(0, node);
            }
            else
            {
                frontier.Insert(int.MaxValue, node);
            }
        }
        backPointers[start] = null;

        HeapElement<int, T> curNode = frontier.ExtractMin();
        while (curNode != null)
        {
            if (!sptSet.Contains(curNode.value))
            {
                List<INodeEdge> outgoingEdges = curNode.value.GetNeighborEdges();
                foreach(INodeEdge edge in outgoingEdges)
                {
                    T neighbor = (T) edge.GetNode();
                    int edgeWeight = edge.GetWeight();
                    if (!sptSet.Contains(neighbor))
                    {
                        frontier.Update(edgeWeight + curNode.key, neighbor);
                        backPointers[neighbor] = curNode.value;
                    }
                }
                sptSet.Add(curNode.value);
            }
            curNode = frontier.ExtractMin();
        }
        return TraceBackPointers(backPointers, end);
    }

    public List<T> TraceBackPointers(Dictionary<T, T> backPointers, T end)
    {
        List<T> path = new List<T>();
        T curNode = end;
        while (curNode != null)
        {
            path.Add(curNode);
            curNode = backPointers[curNode];
        }
        return path;
    }

}

//Assumes navmesh is perpendicular to z axis
public class NavigationMesh : MonoBehaviour
{
    Graph<Triangle> navMeshGraph;
    //TODO: use this instead
    DelaunayMesh mesh;

    private void Awake()
    {
        List<Vector2> verts = new List<Vector2>();
        int C = 4;
        int R = 4;
        Vector2 xBounds = new Vector2(-10.0f, 10.0f);
        Vector2 yBounds = new Vector2(-10.0f, 10.0f);

        float W = (xBounds[1] - xBounds[0]);
        float H = (yBounds[1] - yBounds[0]);

        for(int i = 0; i < R; i++)
        {
            float y = yBounds[0] + i * H / R;
            for(int j = 0; j < C; j++)
            {
                float x = xBounds[0] + j * W / C;
                Debug.Log(x + ", " + y);
                verts.Add(new Vector2(x, y));
            }
        }
        mesh = new DelaunayMesh(verts.ToArray());

        /*
        mesh = new DelaunayMesh(verts.ToArray(),
            new List<Vector2[]> {
                new Vector2[] {
                    //new Vector2(0.5f, 0.5f),
                    //new Vector2(1.5f, 2.0f),
                    //new Vector2(2.2f, 0.8f)
                }
            });
        */

        /*
        Vector2[] verts = new Vector2[]
        {
            new Vector2(0, 3),
            new Vector2(0, 0),
            new Vector2(3, 0),
            new Vector2(3, 2),
            new Vector2(3, 3),
            new Vector2(1, 2),
            new Vector2(2.2f, 0.8f),

            new Vector2(2, 2),
            new Vector2(2, 3),
            new Vector2(1, 0),
            new Vector2(1.5f, 2.0f),
            new Vector2(1, 1),
            new Vector2(2, 1),
            new Vector2(0, 1),

            new Vector2(0, 2),
            new Vector2(1, 3),

            new Vector2(2, 0),

            //Fucks up at this point
            new Vector2(3, 1)
        };
        mesh = new DelaunayMesh(verts);
        */

        navMeshGraph = new Graph<Triangle>(mesh.tris);
    }

    List<Vector2> StringPullingAlgorithm(List<Triangle> triPath, Vector2 startPos, Vector2 targetPos)
    {
        if (triPath.Count == 1)
        {
            return new List<Vector2> { startPos, targetPos };
        }

        List<Vector2> edgePortals = new List<Vector2>();
        /*
        Vector2Int e = triPairToEdgeMap[new Vector2Int(triPath[0], triPath[1])];
        Vector2 v1 = mesh.vertices[e[0]];
        Vector2 v2 = mesh.vertices[e[1]];

        if (VecMath.Det(v1 - startPos, v2 - startPos) >= 0)
        {
            //counter clockwise rotation
            edgePortals.Add(v1);
            edgePortals.Add(v2);
        }
        else
        {
            //clockwise rotation
            edgePortals.Add(v2);
            edgePortals.Add(v1);
        }

        //Make sure that all edge portals are oriented in the same way
        for (int i = 2; i < triPath.Count; i++)
        {
            int tri1 = triPath[i - 1];
            int tri2 = triPath[i];
            e = triPairToEdgeMap[new Vector2Int(tri1, tri2)];
            v1 = mesh.vertices[e[0]];
            v2 = mesh.vertices[e[1]];

            if (v1 == edgePortals[edgePortals.Count - 2] ||
                    v2 == edgePortals[edgePortals.Count - 1])
            {
                edgePortals.Add(v1);
                edgePortals.Add(v2);
            }
            else
            {
                edgePortals.Add(v2);
                edgePortals.Add(v1);
            }
        }
        */

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

        
        for(int i = 0; i < edgePortals.Count; i+=2)
        {
            //Debug.DrawLine(new Vector2(10.0f, 0.0f), transform.TransformPoint(edgePortals[i]), Color.cyan, 0.0f, false);
            Debug.DrawLine(transform.TransformPoint(edgePortals[i]),
                transform.TransformPoint(edgePortals[i + 1]), Color.magenta, 0.0f, false);
        }
        

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
                if (edgePortals[ep] == edgePortals[epL] || VecMath.Det(funnelR, newFunnelL) >= 0)
                {
                    funnelL = newFunnelL;
                    epL = ep;
                }
                else
                {
                    breadCrumbs.Add(breadCrumbs[breadCrumbs.Count - 1] + funnelR);
                    ep = epR;
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
                if (edgePortals[ep + 1] == edgePortals[epR + 1] || VecMath.Det(newFunnelR, funnelL) >= 0)
                {
                    funnelR = newFunnelR;
                    epR = ep;
                }
                else
                {
                    breadCrumbs.Add(breadCrumbs[breadCrumbs.Count - 1] + funnelL);
                    ep = epL;
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
        for (int i = 1; i < breadCrumbs.Count; i++)
        {
            Debug.DrawLine(transform.TransformPoint(breadCrumbs[i - 1]),
                               transform.TransformPoint(breadCrumbs[i]), Color.green, 0.0f, false);
        }

        return breadCrumbs;

    }

    /*
    List<Vector2> StringPullingAlgorithm(List<int> triPath, Vector2 startPos, Vector2 targetPos)
    {
        if (triPath.Count == 1)
        {
            return new List<Vector2> { startPos, targetPos };
        }

        List<Vector2> edgePortals = new List<Vector2>();
        Vector2Int e = triPairToEdgeMap[new Vector2Int(triPath[0], triPath[1])];
        Vector2 v1 = mesh.vertices[e[0]];
        Vector2 v2 = mesh.vertices[e[1]];
        
        if (VecMath.Det(v1 - startPos, v2 - startPos) >= 0)
        {
            //counter clockwise rotation
            edgePortals.Add(v1);
            edgePortals.Add(v2);
        }
        else
        {
            //clockwise rotation
            edgePortals.Add(v2);
            edgePortals.Add(v1);
        }

        //Make sure that all edge portals are oriented in the same way
        for (int i = 2; i < triPath.Count; i++)
        {
            int tri1 = triPath[i - 1];
            int tri2 = triPath[i];
            e = triPairToEdgeMap[new Vector2Int(tri1, tri2)];
            v1 = mesh.vertices[e[0]];
            v2 = mesh.vertices[e[1]];

            if(v1 == edgePortals[edgePortals.Count - 2] || 
                    v2 == edgePortals[edgePortals.Count - 1])
            {
                edgePortals.Add(v1);
                edgePortals.Add(v2);
            }
            else
            {
                edgePortals.Add(v2);
                edgePortals.Add(v1);
            }
        }

        edgePortals.Add(targetPos);
        edgePortals.Add(targetPos);

        //for(int i = 0; i < edgePortals.Count; i+=2)
        //{
        //    Debug.DrawLine(transform.TransformPoint(edgePortals[i]),
        //        transform.TransformPoint(edgePortals[i + 1]), Color.magenta, 0.0f, false);
        //}
        
        //Run Simple Stupid Funnel Algorithm.
        List<Vector2> breadCrumbs = new List<Vector2> { startPos };
        Vector2 funnelL = edgePortals[0] - breadCrumbs[breadCrumbs.Count - 1];
        Vector2 funnelR = edgePortals[1] - breadCrumbs[breadCrumbs.Count - 1];

        //Left funnel refers to left from our point of view looking down on navmesh

        int t = 0;
        int epL = 2;
        int epR = 2;
        int ep = 2;

        while (ep < edgePortals.Count && t < 1000)
        {
            Vector2 newFunnelL = edgePortals[ep] - breadCrumbs[breadCrumbs.Count - 1];
            if (VecMath.Det(funnelL, newFunnelL) >= 0)
            {
                if (edgePortals[ep] == edgePortals[epL] || VecMath.Det(newFunnelL, funnelR) >= 0)
                {
                    funnelL = newFunnelL;
                    epL = ep;
                }
                else
                {
                    breadCrumbs.Add(breadCrumbs[breadCrumbs.Count - 1] + funnelR);
                    ep = epR;
                    epL = ep;
                    epR = ep;
                    funnelL = edgePortals[epL] - breadCrumbs[breadCrumbs.Count - 1];
                    funnelR = edgePortals[epR + 1] - breadCrumbs[breadCrumbs.Count - 1];
                    continue;
                }

            }

            Vector2 newFunnelR = edgePortals[ep + 1] - breadCrumbs[breadCrumbs.Count - 1];
            if (VecMath.Det(funnelR, newFunnelR) <= 0)
            {
                if (edgePortals[ep + 1] == edgePortals[epR + 1] || VecMath.Det(funnelL, newFunnelR) >= 0)
                {
                    funnelR = newFunnelR;
                    epR = ep;
                }
                else
                {
                    breadCrumbs.Add(breadCrumbs[breadCrumbs.Count - 1] + funnelL);
                    ep = epL;
                    epL = ep;
                    epR = ep;
                    funnelL = edgePortals[epL] - breadCrumbs[breadCrumbs.Count - 1];
                    funnelR = edgePortals[epR + 1] - breadCrumbs[breadCrumbs.Count - 1];
                    continue;
                }
            }

            ep += 2;
            t += 1;
        }

        if (t == 100)
        {
            Debug.LogError("FUNNEL ALG FAILED");
        }

        breadCrumbs.Add(targetPos);
        for (int i = 1; i < breadCrumbs.Count; i++)
        {
            Debug.DrawLine(transform.TransformPoint(breadCrumbs[i - 1]),
                               transform.TransformPoint(breadCrumbs[i]), Color.green, 0.0f, false);
        }
        
        return breadCrumbs;
        
    }
    */

    public Vector2[] GetShortestPathFromTo(Vector2 start, Vector2 destination)
    {
        Vector2 startPosLocal = transform.InverseTransformPoint(start);
        Vector2 endPosLocal = transform.InverseTransformPoint(destination);
        Triangle startTri = FindContainingTriangle(startPosLocal);
        Triangle endTri = FindContainingTriangle(endPosLocal);

        List<Triangle> triPath = navMeshGraph.DijkstrasAlgorithm(startTri, endTri);

        /*
        for (int i = 0; i < triPath.Count - 1; i++)
        {
            Triangle curTri = triPath[i];
            Triangle nextTri = triPath[i + 1];
            Vector2 c1 = (curTri.edge.origin.p + curTri.edge.next.origin.p + curTri.edge.next.next.origin.p) / 3;
            Vector2 c2 = (nextTri.edge.origin.p + nextTri.edge.next.origin.p + nextTri.edge.next.next.origin.p) / 3;
            Debug.DrawLine(c1, c2, Color.magenta, 0.0f, false);
        }
        */

        List<Vector2> shortestPathLocal = StringPullingAlgorithm(triPath, startPosLocal, endPosLocal);

        Vector2[] shortestPathWorld = new Vector2[shortestPathLocal.Count - 1];
        for(int i = 1; i < shortestPathLocal.Count; i++)
        {
            shortestPathWorld[i - 1] = transform.TransformPoint(shortestPathLocal[i]);
        }

        return shortestPathWorld;
    }

    private Triangle FindContainingTriangle(Vector2 p)
    {
        return mesh.FindContainingTriangle(p);

    }
    
}


