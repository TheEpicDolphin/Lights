using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GeometryUtils;
using VecUtils;


public class BoundaryEdge
{
    public Vector2 v1;
    public Vector2 v2;
    public Vector2 n;

    public BoundaryEdge(Vector2 v1, Vector2 v2, Vector2 n)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.n = n;
    }
}

public interface INode
{
    void AddNeighbor(int neighbor, int weight);

    List<int> GetNeighbors();

    List<int> GetWeights();
}

//public class NavMeshTriangle : Triangle

public class NavMeshTriangle : INode
{
    public List<int> neighbors;
    public List<int> weights;
    public Vector3[] verts;
    public Vector3 centroid;

    public int idx;
    public List<BoundaryEdge> boundaryEdges;

    public NavMeshTriangle()
    {
        neighbors = new List<int>();
        weights = new List<int>();
        boundaryEdges = new List<BoundaryEdge>();
        verts = new Vector3[3];
        centroid = Vector3.zero;
    }

    public void AddNeighbor(int neighbor, int weight)
    {
        this.neighbors.Add(neighbor);
        this.weights.Add(weight);
    }

    public List<int> GetNeighbors()
    {
        return this.neighbors;
    }

    public List<int> GetWeights()
    {
        return this.weights;
    }
}

public class Graph<T> where T : INode
{
    public T[] nodes;

    public Graph(T[] nodes)
    {
        this.nodes = nodes;
    }

    public int[] DijkstrasAlgorithm(int start)
    {
        HashSet<int> sptSet = new HashSet<int>();
        int[] backPointers = new int[this.nodes.Length];

        MinHeap<int, int> frontier = new MinHeap<int, int>();
        for (int i = 0; i < this.nodes.Length; i++)
        {
            if (i == start)
            {
                frontier.Insert(0, i);
            }
            else
            {
                frontier.Insert(int.MaxValue, i);
            }
        }

        backPointers[start] = -1;
        HeapElement<int, int> curNode = frontier.ExtractMin();
        while (curNode != null)
        {
            if (!sptSet.Contains(curNode.value))
            {
                List<int> neighbors = this.nodes[curNode.value].GetNeighbors();
                List<int> weights = this.nodes[curNode.value].GetWeights();
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int neighbor = neighbors[i];
                    int edgeWeight = weights[i];

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
        return backPointers;
    }

    public List<int> TraceBackPointers(int[] backPointers, int end)
    {
        List<int> path = new List<int>();
        int curNode = end;
        while (curNode != -1)
        {
            path.Add(curNode);
            curNode = backPointers[curNode];

        }
        return path;
    }

    public T BreadthFirstSearch(int start, Func<T, bool> stopCondition)
    {
        HashSet<int> sptSet = new HashSet<int>();

        LinkedList<int> frontier = new LinkedList<int>();
        frontier.AddLast(start);

        LinkedListNode<int> curNode;
        int t = 0;

        while (frontier.Count > 0 && t < 100)
        {
            curNode = frontier.First;
            frontier.RemoveFirst();


            if (!sptSet.Contains(curNode.Value))
            {
                if (stopCondition(this.nodes[curNode.Value]))
                {
                    return this.nodes[curNode.Value];
                }

                List<int> neighbors = this.nodes[curNode.Value].GetNeighbors();
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int neighbor = neighbors[i];

                    if (!sptSet.Contains(neighbor))
                    {
                        frontier.AddLast(neighbor);
                    }
                }
                sptSet.Add(curNode.Value);
            }
            t += 1;

        }
        return default(T);
    }

}

//Assumes navmesh is perpendicular to z axis
public class NavigationMesh : MonoBehaviour
{
    Graph<NavMeshTriangle> navMeshGraph;
    Mesh mesh;
    //TODO: use this instead
    //DelaunayMesh mesh;

    //Maps 2 triangle indices to the edge separating them. The edge is represented by
    //2 vertex indices
    Dictionary<Vector2Int, Vector2Int> triPairToEdgeMap;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        navMeshGraph = NavMeshToGraph();

        /*
        Vector2[] verts = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        */

        int C = 5;
        int R = 5;
        Vector2[] verts = new Vector2[R * C];
        for(int i = 0; i < R; i++)
        {
            for(int j = 0; j < C; j++)
            {
                verts[C * i + j] = new Vector2(i, j);
            }
        }

        //DelaunayMesh dm = new DelaunayMesh(verts);

        DelaunayMesh dm = new DelaunayMesh(verts, 
            new List<Vector2[]> {
                new Vector2[] {
                    new Vector2(0.5f, 0.5f),
                    new Vector2(1.5f, 3.0f),
                    new Vector2(2.3f, -1.5f)
                }
            });
    }

    Graph<NavMeshTriangle> NavMeshToGraph()
    {
        NavMeshTriangle[] navMeshTris = new NavMeshTriangle[mesh.triangles.Length / 3];
        for (int i = 0; i < navMeshTris.Length; i++)
        {
            navMeshTris[i] = new NavMeshTriangle();
        }
        triPairToEdgeMap = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> edgeMap = new Dictionary<Vector2Int, int>();
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int vi1 = mesh.triangles[i];
            int vi2 = mesh.triangles[i + 1];
            int vi3 = mesh.triangles[i + 2];

            Vector2Int e1 = vi1 < vi2 ? new Vector2Int(vi1, vi2) : new Vector2Int(vi2, vi1);
            Vector2Int e2 = vi2 < vi3 ? new Vector2Int(vi2, vi3) : new Vector2Int(vi3, vi2);
            Vector2Int e3 = vi3 < vi1 ? new Vector2Int(vi3, vi1) : new Vector2Int(vi1, vi3);

            int tri = i / 3;
            navMeshTris[tri].idx = tri;
            navMeshTris[tri].centroid = (mesh.vertices[vi1] + mesh.vertices[vi2] + mesh.vertices[vi3]) / 3;
            navMeshTris[tri].verts = new Vector3[] { mesh.vertices[vi1], mesh.vertices[vi2], mesh.vertices[vi3] };
            Vector2 p0 = mesh.vertices[vi1];
            Vector2 p1 = mesh.vertices[vi2];
            Vector2 p2 = mesh.vertices[vi3];

            if (edgeMap.ContainsKey(e1))
            {
                navMeshTris[tri].AddNeighbor(edgeMap[e1], 1);
                navMeshTris[edgeMap[e1]].AddNeighbor(tri, 1);

                triPairToEdgeMap[new Vector2Int(tri, edgeMap[e1])] = e1;
                triPairToEdgeMap[new Vector2Int(edgeMap[e1], tri)] = e1;
                edgeMap.Remove(e1);
            }
            else
            {
                edgeMap[e1] = tri;
            }

            if (edgeMap.ContainsKey(e2))
            {
                navMeshTris[tri].AddNeighbor(edgeMap[e2], 1);
                navMeshTris[edgeMap[e2]].AddNeighbor(tri, 1);

                triPairToEdgeMap[new Vector2Int(tri, edgeMap[e2])] = e2;
                triPairToEdgeMap[new Vector2Int(edgeMap[e2], tri)] = e2;

                edgeMap.Remove(e2);
            }
            else
            {
                edgeMap[e2] = tri;
            }

            if (edgeMap.ContainsKey(e3))
            {
                navMeshTris[tri].AddNeighbor(edgeMap[e3], 1);
                navMeshTris[edgeMap[e3]].AddNeighbor(tri, 1);

                triPairToEdgeMap[new Vector2Int(tri, edgeMap[e3])] = e3;
                triPairToEdgeMap[new Vector2Int(edgeMap[e3], tri)] = e3;

                edgeMap.Remove(e3);
            }
            else
            {
                edgeMap[e3] = tri;
            }
        }

        //The keys that are left in edgeMap correspond to edges that are only part of one triangle. In other words, these edges make up
        //any boundaries of the navigation mesh
        foreach (var e in edgeMap.Keys)
        {
            int tri = edgeMap[e];
            int vi1 = mesh.triangles[3 * tri];
            int vi2 = mesh.triangles[3 * tri + 1];
            int vi3 = mesh.triangles[3 * tri + 2];
            int vi;
            if (vi1 != e[0] && vi1 != e[1])
            {
                vi = vi1;
            }
            else if (vi2 != e[0] && vi2 != e[1])
            {
                vi = vi2;
            }
            else
            {
                vi = vi3;
            }

            //boundaryVerts.Add(e[0]);
            //boundaryVerts.Add(e[1]);

            Vector2 ev0 = mesh.vertices[e[0]];
            Vector2 ev1 = mesh.vertices[e[1]];
            Vector2 v = mesh.vertices[vi];
            Vector2 dir = (ev1 - ev0).normalized;
            Vector2 n = Vector2.Perpendicular(dir);
            if (Vector2.Dot(v - ev0, n) > 0)
            {
                navMeshTris[tri].boundaryEdges.Add(new BoundaryEdge(ev0, ev1, n));
            }
            else
            {
                navMeshTris[tri].boundaryEdges.Add(new BoundaryEdge(ev1, ev0, -n));
            }

        }

        return new Graph<NavMeshTriangle>(navMeshTris);
    }

    /*
     * if(c_origin - source).magnitude > c_radius, it returns the two points that form tangent lines from source to the circle of radius c_radius centered at c_origin. 
     * else if (c_origin - source).magnitude == c_radius, it returns the single point of tangency, twice.
     * else if (c_origin - source).magnitude < c_radius, it returns the intersections of the chord perpendicular to (c_origin - source) and passing through source.
     * 
     * */
    Vector2[] CircleTangentPoints(Vector2 c_origin, float c_radius, Vector2 source)
    {
        Vector2 dp = c_origin - source;
        if (dp.magnitude > c_radius)
        {
            float phi = Mathf.Abs(Mathf.Asin(c_radius / dp.magnitude)) * Mathf.Rad2Deg;
            Vector2 b1 = (Quaternion.AngleAxis(phi, Vector3.forward) * dp).normalized;
            Vector2 b2 = (Quaternion.AngleAxis(-phi, Vector3.forward) * dp).normalized;
            Vector2 p1 = Vector2.Dot(dp, b1) * b1;
            Vector2 p2 = Vector2.Dot(dp, b2) * b2;
            return new Vector2[] { p1 + source, p2 + source };
        }
        else if (dp.magnitude == c_radius)
        {
            return new Vector2[] { source, source };
        }
        else
        {
            dp = source - c_origin;
            float phi = Mathf.Abs(Mathf.Acos(dp.magnitude / c_radius)) * Mathf.Rad2Deg;
            Vector2 b1 = (Quaternion.AngleAxis(phi, Vector3.forward) * dp).normalized;
            Vector2 b2 = (Quaternion.AngleAxis(-phi, Vector3.forward) * dp).normalized;
            Vector2 p1 = Vector2.Dot(dp, b1) * b1;
            Vector2 p2 = Vector2.Dot(dp, b2) * b2;
            // returns chord
            return new Vector2[] { p1 + c_origin, p2 + c_origin };
        }

    }

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

    public Vector2[] GetShortestPathFromTo(Vector2 start, Vector2 destination)
    {
        Vector2 startPosLocal = transform.InverseTransformPoint(start);
        Vector2 endPosLocal = transform.InverseTransformPoint(destination);
        int startNavMeshTriIdx = FindTriContainingPoint(startPosLocal);
        int endNavMeshTriIdx = FindTriContainingPoint(endPosLocal);

        int[] backPointers = navMeshGraph.DijkstrasAlgorithm(endNavMeshTriIdx);
        List<int> triPath = navMeshGraph.TraceBackPointers(backPointers, startNavMeshTriIdx);
        List<Vector2> shortestPathLocal = StringPullingAlgorithm(triPath, startPosLocal, endPosLocal);

        Vector2[] shortestPathWorld = new Vector2[shortestPathLocal.Count - 1];
        for(int i = 1; i < shortestPathLocal.Count; i++)
        {
            shortestPathWorld[i - 1] = transform.TransformPoint(shortestPathLocal[i]);
        }

        return shortestPathWorld;
    }

    private int FindTriContainingPoint(Vector2 p)
    {
        foreach(NavMeshTriangle navMeshTri in navMeshGraph.nodes)
        {
            Vector2 a = navMeshTri.verts[0];
            Vector2 b = navMeshTri.verts[1];
            Vector2 c = navMeshTri.verts[2];
            if(Geometry.IsInTriangle(a, b, c, p))
            {
                return navMeshTri.idx;
            }
        }
        return 0;
    }
    
}


