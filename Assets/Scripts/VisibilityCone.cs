using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VecUtils;
using System.Linq;
using MathUtils;
using AlgorithmUtils;

public class VisibilityCone : MonoBehaviour
{
    //In degrees
    public float angle = 15.0f;
    public int resolution = 5;
    public float coneRadius = 10.0f;

    MeshFilter meshFilt;
    public Color beamColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
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
        Collider2D[] obstacleColliders = Physics2D.OverlapCircleAll(transform.position, coneRadius);
        List<Obstacle> obstacles = new List<Obstacle>();
        foreach(Collider2D obstacleCol in obstacleColliders)
        {
            Obstacle obstacle = obstacleCol.gameObject.GetComponent<Obstacle>();
            if (obstacle)
            {
                obstacles.Add(obstacle);
            }
        }

        List<Vector2> conePoints = Trace(obstacles);
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(Vector3.zero);
        for (int i = 0; i < conePoints.Count; i++)
        {
            vertices.Add(transform.InverseTransformPoint(conePoints[i]));
        }
        
        List<int> indicesList = new List<int>();

        for(int i = 1; i < vertices.Count - 1; i++)
        {
            indicesList.Add(0);
            indicesList.Add(i + 1);
            indicesList.Add(i);
        }

        meshFilt.mesh.Clear();
        meshFilt.mesh.SetVertices(vertices);
        meshFilt.mesh.SetTriangles(indicesList.ToArray(), 0);
        meshFilt.mesh.RecalculateNormals();
        meshFilt.mesh.RecalculateBounds();
    }

    public List<Vector2> Trace(List<Obstacle> obstacles)
    {
        Matrix4x4 fromConeSpace = Matrix4x4.Translate(transform.position);
        fromConeSpace.SetColumn(0, -transform.up);
        fromConeSpace.SetColumn(1, transform.right);
        fromConeSpace.SetColumn(2, transform.forward);
        Matrix4x4 toConeSpace = fromConeSpace.inverse;

        float coneAngle = Mathf.Deg2Rad * angle;
        float thetaL = Mathf.PI + (coneAngle / 2);
        float thetaR = Mathf.PI - (coneAngle / 2);
        List<LinkedListNode<PolarCoord>> sortedKeyVertices = new List<LinkedListNode<PolarCoord>>();

        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleBoundVerts = obstacle.GetWorldBoundVerts(true);
            PolarCoord[] obstacleBoundPolarVerts = new PolarCoord[obstacleBoundVerts.Length];
            //Transform points to cone space
            for(int j = 0; j < obstacleBoundVerts.Length; j++)
            {
                Vector2 v = toConeSpace.MultiplyPoint(obstacleBoundVerts[j]);
                obstacleBoundPolarVerts[j] = PolarCoord.ToPolarCoords(v);
            }

            bool transitionReady = false;
            int i = 0;
            //Wait for first away -> towards transition
            while (i < obstacleBoundPolarVerts.Length)
            {
                PolarCoord p1 = obstacleBoundPolarVerts[i];
                PolarCoord p2 = obstacleBoundPolarVerts[(i + 1) % obstacleBoundPolarVerts.Length];
                if (p1.theta < p2.theta)
                {
                    if (transitionReady)
                    {
                        break;
                    }
                }
                else
                {
                    transitionReady = true;
                }
                i += 1;
            }
            i %= obstacleBoundVerts.Length;

            LinkedList<PolarCoord> contiguousVertices = new LinkedList<PolarCoord>();
            for (int t = 0; t < obstacleBoundPolarVerts.Length; t++)
            {
                PolarCoord p1 = obstacleBoundPolarVerts[i];
                i = (i + 1) % obstacleBoundPolarVerts.Length;
                PolarCoord p2 = obstacleBoundPolarVerts[i];

                //TODO: Must clamp here to keep light within certain radius

                bool isOutOfBounds = (p1.theta > thetaL && p2.theta > thetaL) || 
                                     (p1.theta < thetaR && p2.theta < thetaR);
                if (p1.theta < p2.theta && !isOutOfBounds)
                {
                    //Normal is facing towards beam
                    if (transitionReady)
                    {
                        contiguousVertices = new LinkedList<PolarCoord>();
                        sortedKeyVertices.Add(contiguousVertices.AddLast(p1));
                        transitionReady = false;
                    }

                    sortedKeyVertices.Add(contiguousVertices.AddLast(p2));
                }
                else
                {
                    transitionReady = true;
                }
            }

        }
        
        //Add outer bounds for cone
        LinkedList<PolarCoord> coneBounds = new LinkedList<PolarCoord>();
        float farConeRadius = 100.0f;
        sortedKeyVertices.Add(coneBounds.AddLast(new PolarCoord(farConeRadius, thetaR - 0.01f)));
        float phi = thetaR;
        for (int i = 1; i < resolution; i++)
        {
            phi += coneAngle / resolution;
            PolarCoord p = new PolarCoord(farConeRadius, phi);
            sortedKeyVertices.Add(coneBounds.AddLast(p));
        }
        sortedKeyVertices.Add(coneBounds.AddLast(new PolarCoord(farConeRadius, thetaL + 0.01f)));

        //Order by increasing x and then increasing y
        sortedKeyVertices = sortedKeyVertices
                            .OrderBy(node => node.Value.theta)
                            .ThenBy(node => node.Value.r).ToList();

        List<LinkedListNode<PolarCoord>> activeEdges = new List<LinkedListNode<PolarCoord>>();
        List<PolarCoord> beamFunction = new List<PolarCoord>();

        LinkedListNode<PolarCoord> curClosestEdge = sortedKeyVertices[0];

        foreach (LinkedListNode<PolarCoord> vertNode in sortedKeyVertices)
        {
            if (vertNode.Next != null)
            {
                //This is not an end node
                PolarCoord vs = vertNode.Value;
                if (vertNode.Previous == null)
                {
                    activeEdges.Add(vertNode);
                }
                else
                {
                    int j = activeEdges.IndexOf(vertNode.Previous);
                    activeEdges[j] = vertNode;
                }

                LinkedListNode<PolarCoord> prevClosestEdge = curClosestEdge;

                if (prevClosestEdge.Next == null)
                {
                    Debug.LogError("That one issue");
                }

                PolarCoord clipPrev = PolarCoord.Interpolate(prevClosestEdge.Value, prevClosestEdge.Next.Value, vs.theta);

                PolarCoord closestVert = new PolarCoord(Mathf.Infinity, vs.theta);
                foreach (LinkedListNode<PolarCoord> activeEdge in activeEdges)
                {
                    PolarCoord activeEdgeStart = activeEdge.Value;
                    PolarCoord activeEdgeEnd = activeEdge.Next.Value;
                    PolarCoord clip = PolarCoord.Interpolate(activeEdgeStart, activeEdgeEnd, vs.theta);
                    if (clip.r < closestVert.r)
                    {
                        closestVert.r = clip.r;
                        curClosestEdge = activeEdge;
                    }
                }

                if (prevClosestEdge.Next != curClosestEdge && vertNode == curClosestEdge)
                {
                    //A new edge has started that is closer than the previous closest edge.
                    beamFunction.Add(clipPrev);
                    beamFunction.Add(closestVert);

                }
                else if (vertNode == curClosestEdge)
                {
                    //We are continuing a chain of connected edges
                    beamFunction.Add(closestVert);
                }

            }
            else if (vertNode.Previous != null)
            {
                activeEdges.Remove(vertNode.Previous);
                if (vertNode.Previous == curClosestEdge)
                {
                    //This is an end node
                    PolarCoord ve = vertNode.Value;

                    //Check if this is the end node of the currently closest edge
                    PolarCoord nextClosestVert = new PolarCoord(Mathf.Infinity, ve.theta);
                    foreach (LinkedListNode<PolarCoord> activeEdge in activeEdges)
                    {
                        PolarCoord activeEdgeStart = activeEdge.Value;
                        PolarCoord activeEdgeEnd = activeEdge.Next.Value;
                        PolarCoord clip = PolarCoord.Interpolate(activeEdgeStart, activeEdgeEnd, ve.theta);
                        if(clip.r < nextClosestVert.r)
                        {
                            nextClosestVert.r = clip.r;
                            curClosestEdge = activeEdge;
                        }
                    }

                    beamFunction.Add(vertNode.Value);
                    beamFunction.Add(nextClosestVert);

                }
            }

        }

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

        /*
        List<PolarCoord> aiHidingSpots = new List<PolarCoord>();
        for(int i = 0; i < polarConePoints.Count - 1; i++)
        {
            PolarCoord p1 = polarConePoints[i];
            PolarCoord p2 = polarConePoints[i + 1];
            if (Mathf.Approximately(p1.theta, p2.theta))
            {
                aiHidingSpots.Add();
            }
        }
        */

        List<Vector2> conePoints = new List<Vector2>(); 
        //Transform points from cone space
        for (int j = 0; j < polarConePoints.Count; j++)
        {
            PolarCoord p = polarConePoints[j];
            Vector2 v = fromConeSpace.MultiplyPoint(p.ToCartesianCoordinates());
            conePoints.Add(v);
        }
        return conePoints;

    }
}
