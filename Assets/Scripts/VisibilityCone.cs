using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VecUtils;
using System.Linq;
using MathUtils;
using AlgorithmUtils;

public class VisibilityCone : MonoBehaviour
{
    public float angle = 15.0f;
    public int resolution = 5;
    float coneRadius = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //TODO: add argument for rightHanded/leftHanded coordinate system for appropriate reversing of vertices
    public List<Vector2> Trace(List<Obstacle> obstacles)
    {
        Matrix4x4 toConeSpace = Matrix4x4.Translate(transform.position);
        toConeSpace.SetColumn(0, -transform.up);
        toConeSpace.SetColumn(1, transform.right);
        toConeSpace.SetColumn(2, transform.forward);
        Matrix4x4 fromConeSpace = toConeSpace.inverse;

        Vector2 direction = toConeSpace.GetColumn(0);
        Vector2 origin = toConeSpace.GetColumn(3);

        float thetaL = Mathf.PI + Mathf.Deg2Rad * (angle / 2);
        float thetaR = Mathf.PI - Mathf.Deg2Rad * angle / 2;
        Vector2 coneL = Quaternion.Euler(0, 0,-angle / 2) * direction;
        Vector2 coneR = Quaternion.Euler(0, 0, angle / 2) * direction;
        List<LinkedListNode<PolarCoord>> sortedKeyVertices = new List<LinkedListNode<PolarCoord>>();

        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleBoundVerts = obstacle.GetWorldBoundVerts();
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
                if (p1.theta > p2.theta)
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

                bool isOutOfBounds = (p1.theta > thetaL && p2.theta > thetaL) || 
                                     (p1.theta < thetaR && p2.theta < thetaR);
                if (p1.theta > p2.theta && !isOutOfBounds)
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
        LinkedList<PolarCoord> coneBound = new LinkedList<PolarCoord>();
        float phi = thetaL - 0.01f;
        for (int i = 0; i <= resolution; i++)
        {
            PolarCoord p = new PolarCoord(coneRadius, phi);
            sortedKeyVertices.Add(coneBound.AddLast(p));
            phi += angle / resolution;
        }
        

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
                    Debug.Log("That one issue");
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
        polarConePoints.Add(new PolarCoord(0, 0));

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
            Vector2 v = fromConeSpace.MultiplyPoint(polarConePoints[j].ToCartesianCoordinates());
            conePoints.Add(v);
        }
        return conePoints;

    }
}
