using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VecUtils;
using System.Linq;
using MathUtils;
using AlgorithmUtils;
using GeometryUtils;

public class VisibilityPolygon : MonoBehaviour
{
    public int resolution = 5;
    public float radius = 10.0f;

    MeshFilter meshFilt;
    public Color beamColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
    List<Vector2> outline = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        meshFilt = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRend = gameObject.AddComponent<MeshRenderer>();
        meshRend.material = new Material(Shader.Find("Custom/BeamShader"));
        //meshRend.material = new Material(Shader.Find("Standard"));
        meshRend.material.color = beamColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Draw()
    {
        Collider2D[] obstacleColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        List<Obstacle> obstacles = new List<Obstacle>();
        foreach (Collider2D obstacleCol in obstacleColliders)
        {
            Obstacle obstacle = obstacleCol.gameObject.GetComponent<Obstacle>();
            if (obstacle)
            {
                obstacles.Add(obstacle);
            }
        }

        outline = Trace(obstacles);
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < outline.Count; i++)
        {
            vertices.Add(transform.InverseTransformPoint(outline[i]));
        }

        List<int> indicesList = new List<int>();

        for (int i = 1; i < vertices.Count - 1; i++)
        {
            indicesList.Add(0);
            indicesList.Add(i + 1);
            indicesList.Add(i);
        }

        indicesList.Add(0);
        indicesList.Add(1);
        indicesList.Add(vertices.Count - 1);

        meshFilt.mesh.Clear();
        meshFilt.mesh.SetVertices(vertices);
        meshFilt.mesh.SetTriangles(indicesList.ToArray(), 0);
        meshFilt.mesh.RecalculateNormals();
        meshFilt.mesh.RecalculateBounds();
    }

    public class EdgeNode<T>
    {
        public T Value;
        EdgeNode<T> next;
        System.WeakReference prev;
        public EdgeNode(T val)
        {
            this.Value = val;
        }

        public void ConnectTo(EdgeNode<T> node)
        {
            this.next = node;
            node.prev = new System.WeakReference(this);
        }

        public EdgeNode<T> Next()
        {
            return next;
        }

        public EdgeNode<T> Previous()
        {
            return prev?.Target as EdgeNode<T>;
        }
    }

    //This can be used for flashlights
    public List<Vector2> Sliced(Vector2 direction, float angle)
    {
        Matrix4x4 fromConeSpace = Matrix4x4.Translate(transform.position);
        fromConeSpace.SetColumn(0, -direction);
        fromConeSpace.SetColumn(1, -Vector2.Perpendicular(direction));
        fromConeSpace.SetColumn(2, Vector3.forward);
        Matrix4x4 toConeSpace = fromConeSpace.inverse;

        float coneAngle = Math.Mod(Mathf.Deg2Rad * angle, 2 * Mathf.PI);
        float thetaL = Mathf.PI + (coneAngle / 2);
        float thetaR = Mathf.PI - (coneAngle / 2);


        List<float> beamFunctionThetas = new List<float>();
        foreach (PolarCoord p in beamFunction)
        {
            beamFunctionThetas.Add(p.theta);
        }

        List<PolarCoord> polarConePoints = new List<PolarCoord>();

        //Binary search for left and right bounds (demarcations[i] and demarcations[i + 1])
        int s = Algorithm.BinarySearch(beamFunctionThetas, CompCondition.LARGEST_LEQUAL, thetaR);
        int e = Algorithm.BinarySearch(beamFunctionThetas, CompCondition.SMALLEST_GEQUAL, thetaL);

        PolarCoord clipStart = PolarCoord.Interpolate(beamFunction[s], beamFunction[s + 1], thetaR);
        polarConePoints.Add(clipStart);
        for (int j = s + 1; j < e; j++)
        {
            PolarCoord p = beamFunction[j];
            polarConePoints.Add(p);
        }
        PolarCoord clipEnd = PolarCoord.Interpolate(beamFunction[e - 1], beamFunction[e], thetaL);
        polarConePoints.Add(clipEnd);

        List<Vector2> coneOutline = new List<Vector2>();
        coneOutline.Add(transform.position);
        for (int i = 0; i < polarConePoints.Count; i++)
        {
            Vector2 v = fromConeSpace.MultiplyPoint(polarConePoints[i].ToCartesianCoordinates());
            coneOutline.Add(v);
        }
    }

    public List<Vector2> Trace(List<Obstacle> obstacles)
    {
        Matrix4x4 fromConeSpace = Matrix4x4.Translate(transform.position);
        Matrix4x4 toConeSpace = fromConeSpace.inverse;

        List<EdgeNode<PolarCoord>> sortedEdgeNodes = new List<EdgeNode<PolarCoord>>();

        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleOutlinePoints = obstacle.GetWorldBoundVerts(clockwise: true);
            EdgeNode<PolarCoord>[] obstacleOutlineNodes = new EdgeNode<PolarCoord>[obstacleOutlinePoints.Length];

            bool[] bits = new bool[obstacleOutlineNodes.Length];
            //Convert to nodes
            for (int i = 0; i < obstacleOutlineNodes.Length; i++)
            {
                Vector2 v = toConeSpace.MultiplyPoint(obstacleOutlinePoints[i]);
                obstacleOutlineNodes[i] = new EdgeNode<PolarCoord>(PolarCoord.ToPolarCoords(v));
            }

            for(int i = 0; i < bits.Length; i++)
            {
                EdgeNode<PolarCoord> n1 = obstacleOutlineNodes[i];
                EdgeNode<PolarCoord> n2 = obstacleOutlineNodes[Math.Mod(i + 1, obstacleOutlineNodes.Length)];
                float dt = Math.DeltaAngle(n1.Value.theta, n2.Value.theta);
                //True if normal of edge is facing towards origin
                bits[i] = dt < Mathf.PI && dt > 0.0f;
            }

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    EdgeNode<PolarCoord> n1 = obstacleOutlineNodes[i];
                    EdgeNode<PolarCoord> n2 = obstacleOutlineNodes[Math.Mod(i + 1, obstacleOutlineNodes.Length)];
                    n1.ConnectTo(n2);
                    sortedEdgeNodes.Add(n1);
                    if (!bits[Math.Mod(i + 1, bits.Length)])
                    {
                        sortedEdgeNodes.Add(n2);
                    }
                }
            }

        }

        //Add outer bounds for cone
        float farConeRadius = 100.0f;
        float deltaTheta = 2 * Mathf.PI / resolution;
        float phi = deltaTheta / 2;
        EdgeNode<PolarCoord> startingBoundNode = new EdgeNode<PolarCoord>(new PolarCoord(farConeRadius, phi));
        sortedEdgeNodes.Add(startingBoundNode);
        for (int i = 1; i < resolution; i++)
        {
            phi += deltaTheta;
            EdgeNode<PolarCoord> boundNode = new EdgeNode<PolarCoord>(new PolarCoord(farConeRadius, phi));
            sortedEdgeNodes.Last().ConnectTo(boundNode);
            sortedEdgeNodes.Add(boundNode);
        }
        sortedEdgeNodes.Last().ConnectTo(startingBoundNode);

        //Order by increasing theta and then increasing radius
        sortedEdgeNodes = sortedEdgeNodes
                            .OrderBy(node => node.Value.theta)
                            .ThenBy(node => node.Value.r).ToList();

        List<EdgeNode<PolarCoord>> activeEdges = new List<EdgeNode<PolarCoord>>();
        List<PolarCoord> visibilityFunction = new List<PolarCoord>();

        EdgeNode<PolarCoord> curClosestEdge = startingBoundNode.Previous();
        foreach (EdgeNode<PolarCoord> eNode in sortedEdgeNodes)
        {
            PolarCoord eStart = eNode.Value;
            if(eNode.Next() != null)
            {
                PolarCoord eEnd = eNode.Next().Value;
                if (eStart.theta > Mathf.PI && eEnd.theta < Mathf.PI)
                {
                    activeEdges.Add(eNode);
                    PolarCoord clip = PolarCoord.Interpolate(eStart, eEnd, 0.0f);
                    if (clip.r < curClosestEdge.Value.r)
                    {
                        curClosestEdge = eNode;
                    }
                }
            }
        }

        foreach (EdgeNode<PolarCoord> edgeNode in sortedEdgeNodes)
        {
            if (edgeNode.Next() != null)
            {
                //This is not an end node
                PolarCoord vs = edgeNode.Value;
                if (edgeNode.Previous() == null)
                {
                    activeEdges.Add(edgeNode);
                }
                else
                {
                    int j = activeEdges.IndexOf(edgeNode.Previous());
                    activeEdges[j] = edgeNode;
                }

                EdgeNode<PolarCoord> prevClosestEdge = curClosestEdge;
                PolarCoord clipPrev = PolarCoord.Interpolate(prevClosestEdge.Value, prevClosestEdge.Next().Value, vs.theta);

                PolarCoord closestVert = new PolarCoord(Mathf.Infinity, vs.theta);
                foreach (EdgeNode<PolarCoord> activeEdge in activeEdges)
                {
                    PolarCoord activeEdgeStart = activeEdge.Value;
                    PolarCoord activeEdgeEnd = activeEdge.Next().Value;
                    PolarCoord clip = PolarCoord.Interpolate(activeEdgeStart, activeEdgeEnd, vs.theta);
                    if (clip.r < closestVert.r)
                    {
                        closestVert.r = clip.r;
                        curClosestEdge = activeEdge;
                    }
                }

                if (prevClosestEdge.Next() != curClosestEdge && edgeNode == curClosestEdge)
                {
                    //A new edge has started that is closer than the previous closest edge.
                    visibilityFunction.Add(clipPrev);
                    visibilityFunction.Add(closestVert);

                }
                else if (edgeNode == curClosestEdge)
                {
                    //We are continuing a chain of connected edges
                    visibilityFunction.Add(closestVert);
                }

            }
            else if (edgeNode.Previous() != null)
            {
                activeEdges.Remove(edgeNode.Previous());
                if (edgeNode.Previous() == curClosestEdge)
                {
                    //This is an end node
                    PolarCoord ve = edgeNode.Value;

                    //Check if this is the end node of the currently closest edge
                    PolarCoord nextClosestVert = new PolarCoord(Mathf.Infinity, ve.theta);
                    foreach (EdgeNode<PolarCoord> activeEdge in activeEdges)
                    {
                        PolarCoord activeEdgeStart = activeEdge.Value;
                        PolarCoord activeEdgeEnd = activeEdge.Next().Value;
                        PolarCoord clip = PolarCoord.Interpolate(activeEdgeStart, activeEdgeEnd, ve.theta);
                        if (clip.r < nextClosestVert.r)
                        {
                            nextClosestVert.r = clip.r;
                            curClosestEdge = activeEdge;
                        }
                    }

                    visibilityFunction.Add(edgeNode.Value);
                    visibilityFunction.Add(nextClosestVert);

                }
            }

        }

        List<Vector2> coneOutline = new List<Vector2>();
        coneOutline.Add(transform.position);
        for (int i = 0; i < visibilityFunction.Count; i++)
        {
            Vector2 v = fromConeSpace.MultiplyPoint(visibilityFunction[i].ToCartesianCoordinates());
            coneOutline.Add(v);
        }

        return coneOutline;
    }

    public bool OutlineContainsPoint(Vector2 p)
    {
        return Vector2.Distance(p, transform.position) < radius && 
                Geometry.IsInPolygon(p, outline.ToArray(), counterClockwise: true);
    }

    public bool SliceContainsPoint(Vector2 p, Vector2 dir, float angle)
    {
        //TODO: complete this
        return OutlineContainsPoint(p);
    }

}
