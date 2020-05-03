using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GeometryUtils;


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

public class NavMeshTriangle : INode
{
    public List<int> neighbors;
    public List<int> weights;
    public Vector3[] verts;
    public Vector3 centroid;

    public int idx;
    public float area;
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
    HashSet<int> boundaryVerts = new HashSet<int>();
    Mesh mesh;

    //Maps 2 triangle indices to the edge separating them. The edge is represented by
    //2 vertex indices
    Dictionary<Vector2Int, Vector2Int> triPairToEdgeMap;

    private void Awake()
    {
        navMeshGraph = NavMeshToGraph();
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
            navMeshTris[tri].area = 0.5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);

            if (edgeMap.ContainsKey(e1))
            {
                navMeshTris[tri].AddNeighbor(edgeMap[e1], 1);
                navMeshTris[edgeMap[e1]].AddNeighbor(tri, 1);

                Vector2Int triPair = tri < edgeMap[e1] ? new Vector2Int(tri, edgeMap[e1]) : new Vector2Int(edgeMap[e1], tri);
                triPairToEdgeMap[triPair] = new Vector2Int(vi1, vi2);

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

                Vector2Int triPair = tri < edgeMap[e2] ? new Vector2Int(tri, edgeMap[e2]) : new Vector2Int(edgeMap[e2], tri);
                triPairToEdgeMap[triPair] = new Vector2Int(vi2, vi3);

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

                Vector2Int triPair = tri < edgeMap[e3] ? new Vector2Int(tri, edgeMap[e3]) : new Vector2Int(edgeMap[e3], tri);
                triPairToEdgeMap[triPair] = new Vector2Int(vi3, vi1);

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

            boundaryVerts.Add(e[0]);
            boundaryVerts.Add(e[1]);

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



    List<Vector2> StringPullingAlgorithm(List<int> triPath, Vector2 startPos, Vector2 targetPos, float agentRadius)
    {
        if (triPath.Count == 1)
        {
            return new List<Vector2> { targetPos };
        }

        Vector2Int triPair = triPath[0] < triPath[1] ? new Vector2Int(triPath[0], triPath[1]) : new Vector2Int(triPath[1], triPath[0]);
        Vector2Int prevE = triPairToEdgeMap[triPair];
        Vector2 v1 = mesh.vertices[prevE[0]];
        Vector2 v2 = mesh.vertices[prevE[1]];

        List<Vector2Int> edgePortals = new List<Vector2Int>();
        Dictionary<int, int> vertIndexMap = new Dictionary<int, int> { { prevE[0], 0 }, { prevE[1], 1 } };
        List<Vector2> portalVerts = new List<Vector2> { mesh.vertices[prevE[0]], mesh.vertices[prevE[1]] };

        HashSet<int> mappedBoundaryVerts = new HashSet<int>();

        if (Vector2.SignedAngle(v1 - startPos, v2 - startPos) < 0)
        {
            Vector2Int eMapped = new Vector2Int(vertIndexMap[prevE[0]], vertIndexMap[prevE[1]]);
            edgePortals.Add(eMapped);

            if (boundaryVerts.Contains(prevE[0]))
            {
                mappedBoundaryVerts.Add(0);
            }
            if (boundaryVerts.Contains(prevE[1]))
            {
                mappedBoundaryVerts.Add(1);
            }
        }
        else
        {
            Vector2Int eMapped = new Vector2Int(vertIndexMap[prevE[1]], vertIndexMap[prevE[0]]);
            edgePortals.Add(eMapped);
            prevE = new Vector2Int(prevE[1], prevE[0]);


            if (boundaryVerts.Contains(prevE[0]))
            {
                mappedBoundaryVerts.Add(1);
            }
            if (boundaryVerts.Contains(prevE[1]))
            {
                mappedBoundaryVerts.Add(0);
            }
        }

        for (int i = 2; i < triPath.Count; i++)
        {
            int tri1 = triPath[i - 1];
            int tri2 = triPath[i];
            triPair = tri1 < tri2 ? new Vector2Int(tri1, tri2) : new Vector2Int(tri2, tri1);
            Vector2Int e = triPairToEdgeMap[triPair];

            if (!vertIndexMap.ContainsKey(e[0]))
            {
                vertIndexMap[e[0]] = portalVerts.Count;
                if (boundaryVerts.Contains(e[0]))
                {
                    mappedBoundaryVerts.Add(portalVerts.Count);
                }
                portalVerts.Add(mesh.vertices[e[0]]);
            }

            if (!vertIndexMap.ContainsKey(e[1]))
            {
                vertIndexMap[e[1]] = portalVerts.Count;
                if (boundaryVerts.Contains(e[1]))
                {
                    mappedBoundaryVerts.Add(portalVerts.Count);
                }
                portalVerts.Add(mesh.vertices[e[1]]);
            }

            if (e[1] == prevE[1] || e[0] == prevE[0])
            {
                Vector2Int eMapped = new Vector2Int(vertIndexMap[e[0]], vertIndexMap[e[1]]);
                edgePortals.Add(eMapped);
                prevE = e;
            }
            else if (e[1] == prevE[0] || e[0] == prevE[1])
            {
                Vector2Int eMapped = new Vector2Int(vertIndexMap[e[1]], vertIndexMap[e[0]]);
                edgePortals.Add(eMapped);
                prevE = new Vector2Int(e[1], e[0]);
            }
        }

        Vector2Int lastEdge = edgePortals[edgePortals.Count - 1];
        edgePortals.Add(new Vector2Int(portalVerts.Count, portalVerts.Count));
        portalVerts.Add(targetPos);

        /*
        foreach (Vector2Int edgePortal in edgePortals)
        {
            Debug.DrawLine(portalVerts[edgePortal[0]], portalVerts[edgePortal[1]], Color.clear, 0.0f, false);
        }
        */

        //Run Simple Stupid Funnel Algorithm.
        List<Vector2> breadCrumbs = new List<Vector2> { startPos };

        int n = 6;
        float R = agentRadius / Mathf.Cos(Mathf.PI / n);

        Vector2 minkowskiFunnelL = portalVerts[edgePortals[0][0]] - breadCrumbs[breadCrumbs.Count - 1];
        Vector2 minkowskiFunnelR = portalVerts[edgePortals[0][1]] - breadCrumbs[breadCrumbs.Count - 1];

        if (mappedBoundaryVerts.Contains(edgePortals[0][0]))
        {
            Vector2[] funnelLTangents = CircleTangentPoints(portalVerts[edgePortals[0][0]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);

            if (Vector2.SignedAngle(minkowskiFunnelL, funnelLTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) <= 0)
            {
                minkowskiFunnelL = (R * (funnelLTangents[0] - portalVerts[edgePortals[0][0]]).normalized + portalVerts[edgePortals[0][0]]) - breadCrumbs[breadCrumbs.Count - 1];
            }
            else
            {
                minkowskiFunnelL = (R * (funnelLTangents[1] - portalVerts[edgePortals[0][0]]).normalized + portalVerts[edgePortals[0][0]]) - breadCrumbs[breadCrumbs.Count - 1];
            }
        }

        if (mappedBoundaryVerts.Contains(edgePortals[0][1]))
        {
            Vector2[] funnelRTangents = CircleTangentPoints(portalVerts[edgePortals[0][1]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);

            if (Vector2.SignedAngle(minkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) >= 0)
            {
                minkowskiFunnelR = (R * (funnelRTangents[0] - portalVerts[edgePortals[0][1]]).normalized + portalVerts[edgePortals[0][1]]) - breadCrumbs[breadCrumbs.Count - 1];
            }
            else
            {
                minkowskiFunnelR = (R * (funnelRTangents[1] - portalVerts[edgePortals[0][1]]).normalized + portalVerts[edgePortals[0][1]]) - breadCrumbs[breadCrumbs.Count - 1];
            }
        }


        int epL = 1;
        int epR = 1;
        int ep = 1;

        int t = 0;

        while (ep < edgePortals.Count && t < 1000)
        {

            Vector2 newMinkowskiFunnelL = portalVerts[edgePortals[ep][0]] - breadCrumbs[breadCrumbs.Count - 1];
            if (mappedBoundaryVerts.Contains(edgePortals[ep][0]))
            {
                Vector2[] funnelLTangents = CircleTangentPoints(portalVerts[edgePortals[ep][0]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                if (Vector2.SignedAngle(newMinkowskiFunnelL, funnelLTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) <= 0)
                {
                    newMinkowskiFunnelL = (R * (funnelLTangents[0] - portalVerts[edgePortals[ep][0]]).normalized + portalVerts[edgePortals[ep][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                }
                else
                {
                    newMinkowskiFunnelL = (R * (funnelLTangents[1] - portalVerts[edgePortals[ep][0]]).normalized + portalVerts[edgePortals[ep][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                }

            }

            if (Vector2.SignedAngle(minkowskiFunnelL, newMinkowskiFunnelL) <= 0)
            {
                if (edgePortals[ep][0] == edgePortals[epL][0] || Vector2.SignedAngle(newMinkowskiFunnelL, minkowskiFunnelR) <= 0)
                {
                    minkowskiFunnelL = newMinkowskiFunnelL;
                    epL = ep;
                }
                else
                {
                    breadCrumbs.Add(breadCrumbs[breadCrumbs.Count - 1] + minkowskiFunnelR);
                    ep = epR;
                    epL = ep;
                    epR = ep;
                    minkowskiFunnelL = portalVerts[edgePortals[epL][0]] - breadCrumbs[breadCrumbs.Count - 1];
                    if (mappedBoundaryVerts.Contains(edgePortals[epL][0]))
                    {
                        Vector2[] funnelLTangents = CircleTangentPoints(portalVerts[edgePortals[epL][0]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                        if (Vector2.SignedAngle(minkowskiFunnelL, funnelLTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) <= 0)
                        {
                            minkowskiFunnelL = (R * (funnelLTangents[0] - portalVerts[edgePortals[epL][0]]).normalized + portalVerts[edgePortals[epL][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                        else
                        {
                            minkowskiFunnelL = (R * (funnelLTangents[1] - portalVerts[edgePortals[epL][0]]).normalized + portalVerts[edgePortals[epL][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }

                    }
                    minkowskiFunnelR = portalVerts[edgePortals[epR][1]] - breadCrumbs[breadCrumbs.Count - 1];
                    if (mappedBoundaryVerts.Contains(edgePortals[epR][1]))
                    {
                        Vector2[] funnelRTangents = CircleTangentPoints(portalVerts[edgePortals[epR][1]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                        if (Vector2.SignedAngle(minkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) >= 0)
                        {
                            minkowskiFunnelR = (R * (funnelRTangents[0] - portalVerts[edgePortals[epR][1]]).normalized + portalVerts[edgePortals[epR][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                        else
                        {
                            minkowskiFunnelR = (R * (funnelRTangents[1] - portalVerts[edgePortals[epR][1]]).normalized + portalVerts[edgePortals[epR][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                    }
                    continue;
                }

            }

            Vector2 newMinkowskiFunnelR = portalVerts[edgePortals[ep][1]] - breadCrumbs[breadCrumbs.Count - 1];
            if (mappedBoundaryVerts.Contains(edgePortals[ep][1]))
            {
                Vector2[] funnelRTangents = CircleTangentPoints(portalVerts[edgePortals[ep][1]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                newMinkowskiFunnelR = Vector2.SignedAngle(newMinkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) >= 0 ?
                                funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1] : funnelRTangents[1] - breadCrumbs[breadCrumbs.Count - 1];

                if (Vector2.SignedAngle(newMinkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) >= 0)
                {
                    newMinkowskiFunnelR = (R * (funnelRTangents[0] - portalVerts[edgePortals[ep][1]]).normalized + portalVerts[edgePortals[ep][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                }
                else
                {
                    newMinkowskiFunnelR = (R * (funnelRTangents[1] - portalVerts[edgePortals[ep][1]]).normalized + portalVerts[edgePortals[ep][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                }
            }

            if (Vector2.SignedAngle(minkowskiFunnelR, newMinkowskiFunnelR) >= 0)
            {
                if (edgePortals[ep][1] == edgePortals[epR][1] || Vector2.SignedAngle(minkowskiFunnelL, newMinkowskiFunnelR) <= 0)
                {
                    minkowskiFunnelR = newMinkowskiFunnelR;
                    epR = ep;
                }
                else
                {
                    //breadCrumbs.Add(portalVerts[edgePortals[epL][0]]);
                    breadCrumbs.Add(breadCrumbs[breadCrumbs.Count - 1] + minkowskiFunnelL);
                    ep = epL;
                    epL = ep;
                    epR = ep;
                    minkowskiFunnelL = portalVerts[edgePortals[epL][0]] - breadCrumbs[breadCrumbs.Count - 1];
                    if (mappedBoundaryVerts.Contains(edgePortals[epL][0]))
                    {
                        Vector2[] funnelLTangents = CircleTangentPoints(portalVerts[edgePortals[epL][0]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                        if (Vector2.SignedAngle(minkowskiFunnelL, funnelLTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) <= 0)
                        {
                            minkowskiFunnelL = (R * (funnelLTangents[0] - portalVerts[edgePortals[epL][0]]).normalized + portalVerts[edgePortals[epL][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                        else
                        {
                            minkowskiFunnelL = (R * (funnelLTangents[1] - portalVerts[edgePortals[epL][0]]).normalized + portalVerts[edgePortals[epL][0]]) - breadCrumbs[breadCrumbs.Count - 1];

                        }

                    }
                    minkowskiFunnelR = portalVerts[edgePortals[epR][1]] - breadCrumbs[breadCrumbs.Count - 1];
                    if (mappedBoundaryVerts.Contains(edgePortals[epR][1]))
                    {
                        Vector2[] funnelRTangents = CircleTangentPoints(portalVerts[edgePortals[epR][1]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                        if (Vector2.SignedAngle(minkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1]) >= 0)
                        {
                            minkowskiFunnelR = (R * (funnelRTangents[0] - portalVerts[edgePortals[epR][1]]).normalized + portalVerts[edgePortals[epR][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                        else
                        {
                            minkowskiFunnelR = (R * (funnelRTangents[1] - portalVerts[edgePortals[epR][1]]).normalized + portalVerts[edgePortals[epR][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                    }
                    continue;
                }
            }

            ep += 1;
            t += 1;

        }

        if (t == 100)
        {
            Debug.LogError("FUNNEL ALG FAILED");
        }

        for (int i = 1; i < breadCrumbs.Count; i++)
        {
            Debug.DrawLine(transform.TransformPoint(breadCrumbs[i - 1]),
                               transform.TransformPoint(breadCrumbs[i]), Color.green, 0.0f, false);
        }

        breadCrumbs.Add(targetPos);
        return breadCrumbs;
    }

    public Vector2[] GetShortestPathFromTo(Vector2 start, Vector2 destination, float radius)
    {
        Vector2 startPosLocal = transform.InverseTransformPoint(start);
        Vector2 endPosLocal = transform.InverseTransformPoint(destination);
        int startNavMeshTriIdx = FindTriContainingPoint(start);
        int endNavMeshTriIdx = FindTriContainingPoint(destination);

        int[] backPointers = navMeshGraph.DijkstrasAlgorithm(endNavMeshTriIdx);
        List<int> triPath = navMeshGraph.TraceBackPointers(backPointers, startNavMeshTriIdx);
        List<Vector2> shortestPathLocal = StringPullingAlgorithm(triPath, startPosLocal, endPosLocal, radius);

        Vector2[] shortestPathWorld = new Vector2[shortestPathLocal.Count - 1];
        for(int i = 1; i < shortestPathLocal.Count; i++)
        {
            shortestPathWorld[i] = transform.TransformPoint(shortestPathLocal[i]);
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


