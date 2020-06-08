using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VecUtils;
using System.Linq;

public class VisibilityCone : MonoBehaviour
{
    public struct PolarCoord
    {
        public float r;
        public float theta;
        public PolarCoord(float r, float theta)
        {
            this.r = r;
            this.theta = theta;
        }

        public float x()
        {
            return r * Mathf.Cos(theta);
        }

        public float y()
        {
            return r * Mathf.Sin(theta);
        }
    }

    public float angle = 15.0f;
    float beamRadius = 10.0f;
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
        Vector2 origin = transform.position;
        Vector2 direction = transform.up;
        float thetaL = 180.0f + angle / 2;
        float thetaR = 180.0f - angle / 2;
        Vector2 coneL = Quaternion.Euler(0, 0,-angle / 2) * direction;
        Vector2 coneR = Quaternion.Euler(0, 0, angle / 2) * direction;
        List<LinkedListNode<PolarCoord>> sortedKeyVertices = new List<LinkedListNode<PolarCoord>>();

        foreach (Obstacle obstacle in obstacles)
        {

            Vector2[] obstacleBoundVerts = obstacle.GetWorldBoundVerts(true);

            bool transitionReady = false;
            int i = 0;
            //Wait for first away -> towards transition
            while (i < obstacleBoundVerts.Length)
            {
                Vector2 v1 = obstacleBoundVerts[i];
                Vector2 v2 = obstacleBoundVerts[(i + 1) % obstacleBoundVerts.Length];
                if (VecMath.Det(v1 - origin, v2 - origin) > 0)
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
            for (int t = 0; t < obstacleBoundVerts.Length; t++)
            {
                Vector2 v1 = obstacleBoundVerts[i];
                i = (i + 1) % obstacleBoundVerts.Length;
                Vector2 v2 = obstacleBoundVerts[i];

                PolarCoord pcoord1 = new PolarCoord(Vector2.Distance(v1, origin), VecMath.CounterClockwiseAngle(-direction, v1));
                PolarCoord pcoord2 = new PolarCoord(Vector2.Distance(v2, origin), VecMath.CounterClockwiseAngle(-direction, v2));
                bool isOutOfBounds = (pcoord1.theta > thetaL && pcoord2.theta > thetaL) || 
                                     (pcoord1.theta < thetaR && pcoord2.theta < thetaR);
                if (VecMath.Det(v1 - origin, v2 - origin) > 0 && !isOutOfBounds)
                {
                    //Normal is facing towards beam
                    if (transitionReady)
                    {
                        contiguousVertices = new LinkedList<PolarCoord>();
                        sortedKeyVertices.Add(contiguousVertices.AddLast(pcoord1));
                        transitionReady = false;
                    }

                    sortedKeyVertices.Add(contiguousVertices.AddLast(pcoord2));
                }
                else
                {
                    transitionReady = true;
                }
            }

        }


        LinkedList<ObstacleVertex> topLightBound = new LinkedList<ObstacleVertex>();
        Vector2 vts = new Vector2(lims[0].x - 0.1f, beamLength);
        Vector2 vte = new Vector2(lims[1].x + 0.1f, beamLength);
        sortedKeyVertices.Add(topLightBound.AddLast(new ObstacleVertex(vts, null)));
        sortedKeyVertices.Add(topLightBound.AddLast(new ObstacleVertex(vte, null)));

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
                LineSegment lsPrev = new LineSegment(prevClosestEdge.Value.v, prevClosestEdge.Next.Value.v);
                Vector2 clipPrev = lsPrev.p1 + ((vs.x - lsPrev.p1.x) / lsPrev.dir.x) * lsPrev.dir;
                clipPrev = new Vector2(vs.x, clipPrev.y);

                Vector2 closestVert = new Vector2(vs.x, Mathf.Infinity);
                foreach (LinkedListNode<ObstacleVertex> activeEdge in activeEdges)
                {
                    Vector2 activeEdgeStart = activeEdge.Value.v;
                    Vector2 activeEdgeEnd = activeEdge.Next.Value.v;
                    LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                    Vector2 clip = ls.p1 + ((vs.x - ls.p1.x) / ls.dir.x) * ls.dir;
                    if (clip.y < closestVert.y)
                    {
                        closestVert.y = clip.y;
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
                        Vector2 a = new Vector2(activeEdgeStart.x(), activeEdgeEnd.y()) + origin;
                        Vector2 b = new Vector2(activeEdgeEnd.x(), activeEdgeEnd.y()) + origin;
                        Vector2 c = (ve.theta / ) * a + (ve.theta / ) * b;
                        float clipR = Vector2.Distance(c, origin);
                        if(clipR < nextClosestVert.r)
                        {
                            nextClosestVert.r = clipR;
                            curClosestEdge = activeEdge;
                        }
                    }

                    beamFunction.Add(vertNode.Value);
                    beamFunction.Add(nextClosestVert);

                }
            }

        }

        List<float> beamFunctionXs = new List<float>();
        foreach (PolarCoord p in beamFunction)
        {
            beamFunctionXs.Add(p.v.x);
        }

        List<Vector2> beamComponent = new List<Vector2>();
        List<Tuple<Obstacle, Vector2[]>> illuminatedEdges = new List<Tuple<Obstacle, Vector2[]>>();

        //Binary search for left and right bounds (demarcations[i] and demarcations[i + 1])
        int s = Algorithm.BinarySearch(beamFunctionXs, CompCondition.LARGEST_LEQUAL, lims[0].x);
        int e = Algorithm.BinarySearch(beamFunctionXs, CompCondition.SMALLEST_GEQUAL, lims[1].x);

        Vector2 dir1 = (beamFunction[s + 1].v - beamFunction[s].v).normalized;
        Vector2 clipStart = beamFunction[s].v + ((lims[0].x - beamFunction[s].v.x) / dir1.x) * dir1;

        Vector2 dir2 = (beamFunction[e].v - beamFunction[e - 1].v).normalized;
        Vector2 clipEnd = beamFunction[e - 1].v + ((lims[1].x - beamFunction[e - 1].v.x) / dir2.x) * dir2;

        if (!(lims[0] == clipStart))
        {
            beamComponent.Add(lims[0]);
        }
        beamComponent.Add(clipStart);

        for (int j = s + 1; j < e; j++)
        {
            Obstacle obs = beamFunction[j].obsRef;
            Vector2 v = beamFunction[j].v;
            Vector2 vLast = beamComponent.Last();

            if (!Mathf.Approximately(v.x, vLast.x))
            {
                illuminatedEdges.Add(new Tuple<Obstacle, Vector2[]>(obs,
                                    new Vector2[] { curToBeamLocal.MultiplyPoint3x4(vLast),
                                                    curToBeamLocal.MultiplyPoint3x4(v) }));
            }

            beamComponent.Add(v);
        }

        Obstacle lastObs = beamFunction[e].obsRef;
        if (!Mathf.Approximately(beamComponent.Last().x, clipEnd.x))
        {
            illuminatedEdges.Add(new Tuple<Obstacle, Vector2[]>(lastObs,
                            new Vector2[] { curToBeamLocal.MultiplyPoint3x4(beamComponent.Last()),
                                            curToBeamLocal.MultiplyPoint3x4(clipEnd) }));
        }


        beamComponent.Add(clipEnd);
        if (!(lims[1] == clipEnd))
        {
            beamComponent.Add(lims[1]);
        }

        //Transform every point to beam local coordinates
        for (int i = 0; i < beamComponent.Count; i++)
        {
            beamComponent[i] = curToBeamLocal.MultiplyPoint3x4(beamComponent[i]);
        }

        beamComponents.Add(beamComponent);
        if (maxRecurse == 0)
        {
            return;
        }

        //Do reflections/refractions here
        foreach (Tuple<Obstacle, Vector2[]> illuminatedEdge in illuminatedEdges)
        {
            Obstacle obs = illuminatedEdge.el1;
            Vector2[] newLims = illuminatedEdge.el2;
            if (obs != null)
            {
                obs.Cast(this, newLims, beamLocalToCur, beamLength, maxRecurse - 1, ref beamComponents);
            }

        }


    }
}
