using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GeometryUtils;
using AlgorithmUtils;

public class Tuple<T, V>
{
    public T el1;
    public V el2;

    public Tuple(T el1, V el2)
    {
        this.el1 = el1;
        this.el2 = el2;
    }

}

//Vector.up is forward in 2D

public class Beam : MonoBehaviour
{
    class ObstacleVertex
    {
        public Vector2 v;
        public Obstacle obsRef;
        public ObstacleVertex(Vector2 v, Obstacle obs)
        {
            this.v = v;
            this.obsRef = obs;
        }
    }


    public List<Obstacle> obstacles = new List<Obstacle>();

    ObstacleDetector obstacleDetector;

    MeshFilter meshFilt;
    const float EPSILON = 1e-5f;
    public float beamLength = 20.0f;
    //public float[] xLims = new float[] { -0.5f, 0.5f };
    public Vector2[] lims = new Vector2[] { new Vector2(-0.5f, 0.0f), new Vector2(0.5f, 0.0f) };

    public Color beamColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
    Obstacle dummy;

    // Start is called before the first frame update
    void Start()
    {
        obstacleDetector = GetComponentInChildren<ObstacleDetector>();

        meshFilt = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRend = gameObject.AddComponent<MeshRenderer>();
        meshRend.material = new Material(Shader.Find("Custom/BeamShader"));
        //meshRend.material = new Material(Shader.Find("Standard"));
        meshRend.material.color = beamColor;

        gameObject.AddComponent<EdgeCollider2D>();
        dummy = gameObject.AddComponent<Obstacle>();
        
        //Debug.DrawRay(transform.position, 10.0f * transform.up, Color.magenta, 5.0f);
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indicesList = new List<int>();

        Debug.Log("START");
        List<Vector2>[] beamComponents = Cast(lims);
        foreach(List<Vector2> beamComponent in beamComponents)
        {
            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(beamComponent.ToArray());
            int[] indices = tr.Triangulate();
            //TODO: adjust indices for larger vertices array

            for (int i = 0; i < indices.Length; i++)
            {
                indicesList.Add(indices[i] + vertices.Count);
            }

            for (int i = 0; i < beamComponent.Count; i++)
            {
                Debug.Log(beamComponent[i].ToString("F4"));
                
                vertices.Add(new Vector3(beamComponent[i].x, beamComponent[i].y, 0));
            }
            Debug.Log("----------------");

        }

        meshFilt.mesh.Clear();
        meshFilt.mesh.SetVertices(vertices);
        meshFilt.mesh.SetTriangles(indicesList.ToArray(), 0);
        meshFilt.mesh.RecalculateNormals();
        meshFilt.mesh.RecalculateBounds();
        
    }

    private void FixedUpdate()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, (lims[1].x - lims[0].x)/2, transform.up, 
                                                    beamLength, (1 << 12) | (1 << 13));
        obstacles = new List<Obstacle>();
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.GetComponent<Obstacle>())
            {
                Obstacle obstacle = hit.collider.GetComponent<Obstacle>();

                //Do reflection in here. Maybe part of Cast code goes in here
                //obstacle.Reflect(transform.position, transform.up);

                obstacles.Add(obstacle);
            }
            else if (hit.collider.GetComponent<LightTarget>())
            {
                LightTarget lightTarget = hit.collider.GetComponent<LightTarget>();
                lightTarget.AddPotentialBeam(this);
            }
        }
        
    }

    
    public Vector2[] GetBeamPolygon()
    {
        Vector2[] vertAr = new Vector2[meshFilt.mesh.vertexCount];
        for(int i = 0; i < vertAr.Length; i++)
        {
            vertAr[i] = transform.TransformPoint(meshFilt.mesh.vertices[i]);
        }
        return vertAr;
    }



    //Each array element is a separate beam bound. The array has length > 1 when there are color filters, mirrors,
    //or refractive crystals in the path of the light beam
    private List<Vector2>[] Cast(Vector2[] lims)
    {
        List<LinkedListNode<ObstacleVertex>> sortedKeyVertices = new List<LinkedListNode<ObstacleVertex>>();
        List<Tuple<Vector2, Obstacle>> sourceIntersections = new List<Tuple<Vector2, Obstacle>>();

        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleBoundVerts = obstacle.GetBoundVerts();

            bool transitionReady = false;
            int i = 0;
            //Wait for first away -> towards transition
            while (i < obstacleBoundVerts.Length)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[(i + 1) % obstacleBoundVerts.Length]);
                if (v1.x < v2.x)
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

            LinkedList<ObstacleVertex> contiguousVertices = new LinkedList<ObstacleVertex>();
            for (int t = 0; t < obstacleBoundVerts.Length; t++)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                i = (i + 1) % obstacleBoundVerts.Length;
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[i]);

                
                bool isIntersectingSource = (Geometry.Det(lims[1] - lims[0], v1 - lims[0]) > 0) ^ (Geometry.Det(lims[1] - lims[0], v2 - lims[0]) > 0);
                Vector2 intersection;
                if (isIntersectingSource && Geometry.IntersectLines2D(lims[0], lims[1], v1, v2, out intersection))
                {
                    if (intersection.x > lims[0].x && intersection.x < lims[1].x)
                    {
                        sourceIntersections.Add(new Tuple<Vector2, Obstacle>(intersection, obstacle));
                    }
                    
                }

                bool isOutOfBounds = (v1.x < lims[0].x && v2.x < lims[0].x) || (v1.x > lims[1].x && v2.x > lims[1].x) ||
                    (Geometry.Det(lims[1] - lims[0], v1 - lims[0]) < 0 && Geometry.Det(lims[1] - lims[0], v2 - lims[0]) < 0);

                if (v1.x < v2.x && !isOutOfBounds)
                {
                    //Normal is facing towards beam
                    if (transitionReady)
                    {
                        contiguousVertices = new LinkedList<ObstacleVertex>();
                        sortedKeyVertices.Add(contiguousVertices.AddLast(new ObstacleVertex(v1, obstacle)));
                        transitionReady = false;
                    }

                    sortedKeyVertices.Add(contiguousVertices.AddLast(new ObstacleVertex(v2, obstacle)));
                }
                else
                {
                    transitionReady = true;
                }
            }

        }

        //List<Vector2> demarcations = new List<Vector2>() { lims[0], lims[1] };
        
        //Sort intersections with obstacles and beam source by x
        sourceIntersections.OrderBy(intersection => intersection.el1.x);
        HashSet<Obstacle> containingObstacles = new HashSet<Obstacle>();
        Vector2 lims0World = transform.TransformPoint(lims[0]);
        foreach (Obstacle obst in obstacles)
        {
            if (Geometry.IsInPolygon(lims0World, obst.GetBoundVerts()))
            {
                //Get the obstacles that currently contain the leftmost point
                containingObstacles.Add(obst);
            }
        }

        
        List<Vector2> demarcations = new List<Vector2>();
        bool inObstacle = containingObstacles.Count > 0;
        if (!inObstacle)
        {
            demarcations.Add(lims[0]);
        }

        foreach (Tuple<Vector2, Obstacle> intersection in sourceIntersections)
        {
            Vector2 v = intersection.el1;
            Obstacle obst = intersection.el2;

            if (containingObstacles.Contains(obst))
            {
                containingObstacles.Remove(obst);
            }
            else
            {
                containingObstacles.Add(obst);
            }

            if (inObstacle && containingObstacles.Count == 0)
            {
                inObstacle = false;
                demarcations.Add(v);
            }
            else if (!inObstacle && containingObstacles.Count > 0)
            {
                inObstacle = true;
                demarcations.Add(v);
            }
        }

        if (!inObstacle)
        {
            demarcations.Add(lims[1]);
            inObstacle = true;
        }
        


        LinkedList<ObstacleVertex> topLightBound = new LinkedList<ObstacleVertex>();
        Vector2 vts = new Vector2(lims[0].x - 0.1f, beamLength);
        Vector2 vte = new Vector2(lims[1].x + 0.1f, beamLength);
        sortedKeyVertices.Add(topLightBound.AddLast(new ObstacleVertex(vts, dummy)));
        sortedKeyVertices.Add(topLightBound.AddLast(new ObstacleVertex(vte, dummy)));


        //Order by increasing x and then increasing y
        sortedKeyVertices = sortedKeyVertices.OrderBy(node => node.Value.v.x).ThenBy(node => node.Value.v.y).ToList();

        List<LinkedListNode<ObstacleVertex>> activeEdges = new List<LinkedListNode<ObstacleVertex>>();
        List<Vector2> beamFunction = new List<Vector2>();

        LinkedListNode<ObstacleVertex> curClosestEdge = sortedKeyVertices[0];

        foreach (LinkedListNode<ObstacleVertex> vertNode in sortedKeyVertices)
        {
            if (vertNode.Next != null)
            {
                //This is not an end node
                Vector2 vs = vertNode.Value.v;

                if (vertNode.Previous == null)
                {
                    activeEdges.Add(vertNode);
                }
                else
                {
                    int j = activeEdges.IndexOf(vertNode.Previous);
                    activeEdges[j] = vertNode;
                }

                LinkedListNode<ObstacleVertex> prevClosestEdge = curClosestEdge;
                LineSegment lsPrev = new LineSegment(prevClosestEdge.Value.v, prevClosestEdge.Next.Value.v);
                Vector2 clipPrev = lsPrev.p1 + ((vs.x - lsPrev.p1.x) / lsPrev.dir.x) * lsPrev.dir;

                Vector2 closestVert = new Vector2(vs.x, Mathf.Infinity);
                foreach (LinkedListNode<ObstacleVertex> activeEdge in activeEdges)
                {
                    Vector2 activeEdgeStart = activeEdge.Value.v;
                    Vector2 activeEdgeEnd = activeEdge.Next.Value.v;
                    LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                    Vector2 clip = ls.p1 + ((vs.x - ls.p1.x) / ls.dir.x) * ls.dir;
                    if (clip.y < closestVert.y)
                    {
                        //closestVert = clip;
                        closestVert.y = clip.y;
                        curClosestEdge = activeEdge;
                    }
                }

                if (prevClosestEdge.Next != curClosestEdge && vertNode == curClosestEdge)
                {
                    beamFunction.Add(clipPrev);
                }
                if (vertNode == curClosestEdge)
                {
                    beamFunction.Add(closestVert);
                }

            }
            else if (vertNode.Previous != null)
            {
                activeEdges.Remove(vertNode.Previous);
                if (vertNode.Previous == curClosestEdge)
                {
                    //This is an end node
                    Vector2 ve = vertNode.Value.v;

                    //Check if this is the end node of the currently closest edge
                    Vector2 nextClosestVert = new Vector2(ve.x, Mathf.Infinity);
                    foreach (LinkedListNode<ObstacleVertex> activeEdge in activeEdges)
                    {
                        Vector2 activeEdgeStart = activeEdge.Value.v;
                        Vector2 activeEdgeEnd = activeEdge.Next.Value.v;
                        LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                        Vector2 clip = ls.p1 + ((ve.x - ls.p1.x) / ls.dir.x) * ls.dir;
                        if (clip.y < nextClosestVert.y)
                        {
                            nextClosestVert = clip;
                            curClosestEdge = activeEdge;
                        }
                    }

                    beamFunction.Add(ve);
                    beamFunction.Add(nextClosestVert);

                }
            }

        }

        Debug.Log("FUNCTION");
        List<float> beamFunctionXs = new List<float>();
        foreach(Vector2 p in beamFunction)
        {
            Debug.Log(p);
            beamFunctionXs.Add(p.x);
        }

        List<Vector2>[] beamComponents = new List<Vector2>[demarcations.Count / 2];

        for (int i = 0; i < demarcations.Count; i += 2)
        {
            //Binary search for left and right bounds (demarcations[i] and demarcations[i + 1])
            int s = Algorithm.BinarySearch(beamFunctionXs, CompCondition.LARGEST_LEQUAL, demarcations[i].x);
            int e = Algorithm.BinarySearch(beamFunctionXs, CompCondition.SMALLEST_GEQUAL, demarcations[i + 1].x);
            Vector2 dir1 = (beamFunction[s + 1] - beamFunction[s]).normalized;
            Vector2 clipStart = beamFunction[s] + ((demarcations[i].x - beamFunction[s].x) / dir1.x) * dir1;
            //Debug.Log(beamFunction[s].ToString("F4"));
            //Debug.Log(beamFunction[s + 1].ToString("F4"));
            Debug.Log(s);
            Debug.Log(e);

            Vector2 dir2 = (beamFunction[e + 1] - beamFunction[e]).normalized;
            Vector2 clipEnd = beamFunction[e] + ((demarcations[i + 1].x - beamFunction[e].x) / dir2.x) * dir2;
            
            beamComponents[i / 2] = new List<Vector2>() { demarcations[i], clipStart };

            for (int j = s + 1; j <= e; j++)
            {
                beamComponents[i / 2].Add(beamFunction[j]);
            }
            beamComponents[i / 2].Add(clipEnd);
            beamComponents[i / 2].Add(demarcations[i + 1]);
        }

        /*
        for(int i = 0; i < demarcations.Count; i += 2)
        {
            //Binary search for left and right bounds (demarcations[i] and demarcations[i + 1])
            int s = Algorithm.BinarySearch(beamFunctionXs, CompCondition.LESS_THAN, demarcations[i].x);
            int e = Algorithm.BinarySearch(beamFunctionXs, CompCondition.LESS_THAN, demarcations[i + 1].x);
            Vector2 dir1 = (beamFunction[s + 1] - beamFunction[s]).normalized;
            Vector2 clipStart = beamFunction[s] + ((demarcations[i].x - beamFunction[s].x) / dir1.x) * dir1;
            Debug.Log(beamFunction[s].ToString("F4"));
            Debug.Log(clipStart.ToString("F4"));

            Vector2 dir2 = (beamFunction[e + 1] - beamFunction[e]).normalized;
            Vector2 clipEnd = beamFunction[e] + ((demarcations[i + 1].x - beamFunction[e].x) / dir2.x) * dir2;

            beamComponents[i / 2] = new List<Vector2>() { demarcations[i], clipStart };
            for(int j = s + 1; j <= e; j++)
            {
                beamComponents[i / 2].Add(beamFunction[j]);
            }
            beamComponents[i / 2].Add(clipEnd);
            beamComponents[i / 2].Add(demarcations[i + 1]);
        }
        */

        return beamComponents;
    }

    /*
    private List<Vector2>[] Cast(Vector2[] lims)
    {
        List<LinkedListNode<ObstacleVertex>> sortedKeyVertices = new List<LinkedListNode<ObstacleVertex>>();

        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleBoundVerts = obstacle.GetBoundVerts();

            bool transitionReady = false;
            int i = 0;
            //Wait for first away -> towards transition
            while (i < obstacleBoundVerts.Length)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[(i + 1) % obstacleBoundVerts.Length]);
                if (v1.x < v2.x)
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

            LinkedList<ObstacleVertex> contiguousVertices = new LinkedList<ObstacleVertex>();
            for (int t = 0; t < obstacleBoundVerts.Length; t++)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                i = (i + 1) % obstacleBoundVerts.Length;
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[i]);

                bool outOfBounds = (v1.x < lims[0].x && v2.x < lims[0].x) || (v1.x > lims[1].x && v2.x > lims[1].x) ||
                    (Geometry.Det(lims[1] - lims[0], v1 - lims[0]) < 0 && Geometry.Det(lims[1] - lims[0], v2 - lims[0]) < 0);
                if (v1.x < v2.x && !outOfBounds)
                {
                    Vector2 intersection;
                    if (transitionReady)
                    {
                        contiguousVertices = new LinkedList<ObstacleVertex>();

                        bool belowToAbove = (Geometry.Det(lims[1] - lims[0], v1 - lims[0]) < 0) &
                                                    (Geometry.Det(lims[1] - lims[0], v2 - lims[0]) > 0);
                        if (belowToAbove && Geometry.IntersectLines2D(lims[0], lims[1], v1, v2, out intersection))
                        {
                            sortedKeyVertices.Add(contiguousVertices.AddLast(new ObstacleVertex(intersection, obstacle, true)));
                        }
                        else
                        {
                            sortedKeyVertices.Add(contiguousVertices.AddLast(new ObstacleVertex(v1, obstacle, false)));
                        }
                        
                        transitionReady = false;
                    }

                    bool aboveToBelow = (Geometry.Det(lims[1] - lims[0], v1 - lims[0]) > 0) &
                                                    (Geometry.Det(lims[1] - lims[0], v2 - lims[0]) < 0);
                    if (aboveToBelow && Geometry.IntersectLines2D(lims[0], lims[1], v1, v2, out intersection))
                    {
                        sortedKeyVertices.Add(contiguousVertices.AddLast(new ObstacleVertex(intersection, obstacle, true)));
                        transitionReady = true;
                    }
                    else
                    {
                        sortedKeyVertices.Add(contiguousVertices.AddLast(new ObstacleVertex(v2, obstacle, false)));
                    }

                }
                else
                {
                    transitionReady = true;
                }
            }

        }

        LinkedList<ObstacleVertex> leftLightBound = new LinkedList<ObstacleVertex>();
        Vector2 vls = new Vector2(lims[0].x, lims[0].y);
        Vector2 vle = new Vector2(lims[0].x + EPSILON, lims[0].y + EPSILON);
        sortedKeyVertices.Add(leftLightBound.AddLast(new ObstacleVertex(vls, dummy, true)));
        sortedKeyVertices.Add(leftLightBound.AddLast(new ObstacleVertex(vle, dummy, false)));

        LinkedList<ObstacleVertex> rightLightBound = new LinkedList<ObstacleVertex>();
        Vector2 vrs = new Vector2(lims[1].x - EPSILON, lims[1].y + EPSILON);
        Vector2 vre = new Vector2(lims[1].x, lims[1].y);
        sortedKeyVertices.Add(rightLightBound.AddLast(new ObstacleVertex(vrs, dummy, false)));
        sortedKeyVertices.Add(rightLightBound.AddLast(new ObstacleVertex(vre, dummy, true)));

        
        LinkedList<ObstacleVertex> topLightBound = new LinkedList<ObstacleVertex>();
        Vector2 vts = new Vector2(lims[0].x - 2 * EPSILON, beamLength);
        Vector2 vte = new Vector2(lims[1].x + 2 * EPSILON, beamLength);
        sortedKeyVertices.Add(topLightBound.AddLast(new ObstacleVertex(vts, dummy, false)));
        sortedKeyVertices.Add(topLightBound.AddLast(new ObstacleVertex(vte, dummy, false)));
        

        //Order by increasing x and then increasing y
        sortedKeyVertices = sortedKeyVertices.OrderBy(node => node.Value.v.x).ThenBy(node => node.Value.v.y).ToList();


        HashSet<Obstacle> containingObstacles = new HashSet<Obstacle>();
        Vector2 lims0World = transform.TransformPoint(lims[0]);
        foreach (Obstacle obst in obstacles)
        {
            if (Geometry.IsInPolygon(lims0World, obst.GetBoundVerts()))
            {
                //Get the obstacles that currently contain the leftmost point
                containingObstacles.Add(obst);
            }
        }

        bool inObstacle = true;
        containingObstacles.Add(dummy);

        List<LinkedListNode<ObstacleVertex>> activeEdges = new List<LinkedListNode<ObstacleVertex>>();
        List<List<Vector2>> beamComponents = new List<List<Vector2>>();

        LinkedListNode<ObstacleVertex> curClosestEdge = sortedKeyVertices[0];

        foreach (LinkedListNode<ObstacleVertex> vertNode in sortedKeyVertices)
        {            
            if (vertNode.Next != null)
            {
                //This is not an end node
                Vector2 vs = vertNode.Value.v;

                if (vertNode.Previous == null)
                {
                    activeEdges.Add(vertNode);
                }
                else
                {
                    int j = activeEdges.IndexOf(vertNode.Previous);
                    activeEdges[j] = vertNode;
                }

                LinkedListNode<ObstacleVertex> prevClosestEdge = curClosestEdge;
                Debug.Log(prevClosestEdge.Value.v);
                LineSegment lsPrev = new LineSegment(prevClosestEdge.Value.v, prevClosestEdge.Next.Value.v);
                Vector2 clipPrev = lsPrev.p1 + ((vs.x - lsPrev.p1.x) / lsPrev.dir.x) * lsPrev.dir;

                Vector2 closestVert = new Vector2(vs.x, Mathf.Infinity);
                foreach (LinkedListNode<ObstacleVertex> activeEdge in activeEdges)
                {
                    Vector2 activeEdgeStart = activeEdge.Value.v;
                    Vector2 activeEdgeEnd = activeEdge.Next.Value.v;
                    LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                    Vector2 clip = ls.p1 + ((vs.x - ls.p1.x) / ls.dir.x) * ls.dir;
                    if (clip.y < closestVert.y)
                    {
                        closestVert = clip;
                        curClosestEdge = activeEdge;
                    }
                }

                if (!inObstacle)
                {
                    if (prevClosestEdge.Next != curClosestEdge && vertNode == curClosestEdge)
                    {
                        beamComponents.Last().Add(clipPrev);
                    }
                    if (vertNode == curClosestEdge)
                    {
                        beamComponents.Last().Add(closestVert);
                    }
                }

                if (vertNode.Value.isBoundary)
                {
                    if (containingObstacles.Contains(vertNode.Value.obsRef))
                    {
                        containingObstacles.Remove(vertNode.Value.obsRef);
                    }

                    if (containingObstacles.Count == 0)
                    {
                        beamComponents.Add(new List<Vector2>() { vertNode.Value.v });
                        inObstacle = false;
                    }
                }

            }
            else if(vertNode.Previous != null)
            {
                if (vertNode.Value.isBoundary)
                {
                    if (containingObstacles.Count == 0)
                    {
                        beamComponents.Last().Add(vertNode.Value.v);
                        inObstacle = true;
                    }

                    if (!containingObstacles.Contains(vertNode.Value.obsRef))
                    {
                        containingObstacles.Add(vertNode.Value.obsRef);
                    }

                }

                activeEdges.Remove(vertNode.Previous);
                if (vertNode.Previous == curClosestEdge)
                {
                    //This is an end node
                    Vector2 ve = vertNode.Value.v;

                    //Check if this is the end node of the currently closest edge
                    Vector2 nextClosestVert = new Vector2(ve.x, Mathf.Infinity);
                    foreach (LinkedListNode<ObstacleVertex> activeEdge in activeEdges)
                    {
                        Vector2 activeEdgeStart = activeEdge.Value.v;
                        Vector2 activeEdgeEnd = activeEdge.Next.Value.v;
                        LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                        Vector2 clip = ls.p1 + ((ve.x - ls.p1.x) / ls.dir.x) * ls.dir;
                        if (clip.y < nextClosestVert.y)
                        {
                            nextClosestVert = clip;
                            curClosestEdge = activeEdge;
                        }
                    }

                    if (!inObstacle)
                    {
                        beamComponents.Last().Add(ve);
                        beamComponents.Last().Add(nextClosestVert);
                    }

                }
            }
            
        }

        return beamComponents.ToArray();
    }
    */

    /*
    private List<Vector2>[] Cast(Vector2[] lims)
    {

        List<Tuple<float, Obstacle>> beamSourceIntersections = new List<Tuple<float, Obstacle>>();
        List<LinkedListNode<Vector2>> sortedKeyVertices = new List<LinkedListNode<Vector2>>();

        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleBoundVerts = obstacle.GetBoundVerts();

            bool transitionReady = false;
            int i = 0;
            //Wait for first away -> towards transition
            while (i < obstacleBoundVerts.Length)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[(i + 1) % obstacleBoundVerts.Length]);
                if (v1.x < v2.x)
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

            LinkedList<Vector2> contiguousVertices = new LinkedList<Vector2>();
            for(int t = 0; t < obstacleBoundVerts.Length; t++)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                i = (i + 1) % obstacleBoundVerts.Length;
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[i]);

                bool potentialIntersection = (Geometry.Det(lims[1] - lims[0], v1 - lims[0]) > 0) ^ (Geometry.Det(lims[1] - lims[0], v2 - lims[0]) > 0);
                Vector2 intersection;
                //Debug.Log(lims[0].ToString("f4") + ", " + lims[1].ToString("f4"));
                //Debug.Log(v1.ToString("f4") + ", " + v2.ToString("f4"));
                //Debug.Log(potentialIntersection);
                //Debug.Log(Geometry.IntersectLines2D(lims[0], lims[1], v1, v2, out intersection));
                if (potentialIntersection && Geometry.IntersectLines2D(lims[0], lims[1], v1, v2, out intersection))
                {
                    if (intersection.x > lims[0].x && intersection.x < lims[1].x)
                    {
                        beamSourceIntersections.Add(new Tuple<float, Obstacle>(intersection.x, obstacle));
                    }
                }

                bool outOfBounds = (v1.x < lims[0].x && v2.x < lims[0].x) || (v1.x > lims[1].x && v2.x > lims[1].x) ||
                    (Geometry.Det(lims[1] - lims[0], v1 - lims[0]) < 0 && Geometry.Det(lims[1] - lims[0], v2 - lims[0]) < 0);
                if (v1.x < v2.x && !outOfBounds)
                {
                    if (transitionReady)
                    {
                        contiguousVertices = new LinkedList<Vector2>();
                        sortedKeyVertices.Add(contiguousVertices.AddLast(v1));
                        transitionReady = false;
                    }

                    LinkedListNode<Vector2> vNode = contiguousVertices.AddLast(v2);
                    sortedKeyVertices.Add(vNode);

                }
                else
                {
                    transitionReady = true;
                }
            }

        }

        beamSourceIntersections.Sort();
        HashSet<Obstacle> containingObstacles = new HashSet<Obstacle>();
        Vector2 lims0World = transform.TransformPoint(lims[0]);
        foreach (Obstacle obst in obstacles)
        {
            if (Geometry.IsInPolygon(lims0World, obst.GetBoundVerts()))
            {
                //Get the obstacles that currently contain the leftmost point
                containingObstacles.Add(obst);
            }
        }

        //List<float> demarcations = new List<float>() { lims[0].x, lims[1].x };
        List<float> demarcations = new List<float>();
        bool inObstacle = containingObstacles.Count > 0;
        if (!inObstacle)
        {
            demarcations.Add(lims[0].x);
        }

        foreach(Tuple<float, Obstacle> intersection in beamSourceIntersections)
        {
            float t = intersection.el1;
            Obstacle obst = intersection.el2;
            
            if (containingObstacles.Contains(obst))
            {
                containingObstacles.Remove(obst);
            }
            else
            {
                containingObstacles.Add(obst);
            }

            if (inObstacle && containingObstacles.Count == 0)
            {
                inObstacle = false;
                demarcations.Add(t);
            }
            else if (!inObstacle && containingObstacles.Count > 0)
            {
                inObstacle = true;
                demarcations.Add(t);
            }
        }

        if (!inObstacle)
        {
            demarcations.Add(lims[1].x);
            inObstacle = true;
        }


        LinkedList<Vector2> leftLightBound = new LinkedList<Vector2>();
        Vector2 vls = new Vector2(lims[0].x - EPSILON, -EPSILON);
        Vector2 vle = new Vector2(lims[0].x + EPSILON, EPSILON);
        sortedKeyVertices.Add(leftLightBound.AddLast(vls));
        sortedKeyVertices.Add(leftLightBound.AddLast(vle));

        LinkedList<Vector2> rightLightBound = new LinkedList<Vector2>();
        Vector2 vrs = new Vector2(lims[1].x - EPSILON, EPSILON);
        Vector2 vre = new Vector2(lims[1].x + EPSILON, -EPSILON);
        sortedKeyVertices.Add(rightLightBound.AddLast(vrs));
        sortedKeyVertices.Add(rightLightBound.AddLast(vre));


        LinkedList<Vector2> topLightBound = new LinkedList<Vector2>();
        Vector2 vts = new Vector2(lims[0].x - 2 * EPSILON, beamLength);
        Vector2 vte = new Vector2(lims[1].x + 2 * EPSILON, beamLength);
        sortedKeyVertices.Add(topLightBound.AddLast(vts));
        sortedKeyVertices.Add(topLightBound.AddLast(vte));

        //Order by increasing x and then increasing y
        sortedKeyVertices = sortedKeyVertices.OrderBy(node => node.Value.x).ThenBy(node => node.Value.y).ToList();

        
        bool blocked = true;

        List<LinkedListNode<Vector2>> activeEdges = new List<LinkedListNode<Vector2>>();
        List<Vector2>[] beamComponents = new List<Vector2>[demarcations.Count / 2];
        int beamIdx = -1;

        LinkedListNode<Vector2> curClosestEdge = sortedKeyVertices[0];

        foreach (LinkedListNode<Vector2> vertNode in sortedKeyVertices)
        {
            if (demarcations.Count == 0)
            {
                break;
            }

            if (vertNode.Next != null)
            {
                //This is not an end node
                Vector2 vs = vertNode.Value;

                if (vertNode.Previous == null)
                {
                    activeEdges.Add(vertNode);
                }
                else
                {
                    int j = activeEdges.IndexOf(vertNode.Previous);
                    activeEdges[j] = vertNode;
                }

                LinkedListNode<Vector2> prevClosestEdge = curClosestEdge;

                LineSegment lsPrev = new LineSegment(prevClosestEdge.Value, prevClosestEdge.Next.Value);
                Vector2 clipPrev = lsPrev.p1 + ((vs.x - lsPrev.p1.x) / lsPrev.dir.x) * lsPrev.dir;

                Vector2 closestVert = new Vector2(vs.x, Mathf.Infinity);
                foreach (LinkedListNode<Vector2> activeEdge in activeEdges)
                {
                    Vector2 activeEdgeStart = activeEdge.Value;
                    Vector2 activeEdgeEnd = activeEdge.Next.Value;
                    LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                    Vector2 clip = ls.p1 + ((vs.x - ls.p1.x) / ls.dir.x) * ls.dir;
                    if (clip.y < closestVert.y)
                    {
                        closestVert = clip;
                        curClosestEdge = activeEdge;
                    }
                }

                if (vs.x >= demarcations[0])
                {
                    if (blocked)
                    {
                        beamIdx += 1;
                        beamComponents[beamIdx] = new List<Vector2>();
                    }
                    beamComponents[beamIdx].Add(new Vector2(demarcations[0], 0.0f));
                    //beamBounds.Add(new Vector2(demarcations[0], 0.0f));
                    blocked = !blocked;
                    demarcations.RemoveAt(0);
                }

                if (!blocked)
                {
                    
                    if (prevClosestEdge.Next != curClosestEdge && vertNode == curClosestEdge)
                    {
                        //beamBounds.Add(clipPrev);
                        beamComponents[beamIdx].Add(clipPrev);
                    }
                    if (vertNode == curClosestEdge)
                    {
                        //beamBounds.Add(closestVert);
                        beamComponents[beamIdx].Add(closestVert);
                    }
                }
                
            }
            else
            {
                activeEdges.Remove(vertNode.Previous);
                if (vertNode.Previous == curClosestEdge)
                {
                    //This is an end node
                    Vector2 ve = vertNode.Value;

                    //Check if this is the end node of the currently closest edge
                    Vector2 nextClosestVert = new Vector2(ve.x, Mathf.Infinity);
                    foreach (LinkedListNode<Vector2> activeEdge in activeEdges)
                    {
                        Vector2 activeEdgeStart = activeEdge.Value;
                        Vector2 activeEdgeEnd = activeEdge.Next.Value;
                        LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                        Vector2 clip = ls.p1 + ((ve.x - ls.p1.x) / ls.dir.x) * ls.dir;
                        if (clip.y < nextClosestVert.y)
                        {
                            nextClosestVert = clip;
                            curClosestEdge = activeEdge;
                        }
                    }

                    if (ve.x >= demarcations[0])
                    {
                        if (blocked)
                        {
                            beamIdx += 1;
                            beamComponents[beamIdx] = new List<Vector2>();
                        }
                        beamComponents[beamIdx].Add(new Vector2(demarcations[0], 0.0f));
                        //beamBounds.Add(new Vector2(demarcations[0], 0.0f));
                        blocked = !blocked;
                        demarcations.RemoveAt(0);
                    }

                    if (!blocked)
                    {
                        //beamBounds.Add(ve);
                        //beamBounds.Add(nextClosestVert);
                        beamComponents[beamIdx].Add(ve);
                        beamComponents[beamIdx].Add(nextClosestVert);
                    }
                                      
                }

            }
        }

        //return new List<Vector2>[] { beamBounds };
        return beamComponents;
    }
    */

    /*
    private List<Vector2>[] Cast(Vector2[] lims)
    {

        List<LinkedListNode<Vector2>> sortedKeyVertices = new List<LinkedListNode<Vector2>>();
        List<Vector2[]> intersectedObstacles = new List<Vector2[]>();

        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleBoundVerts = obstacle.GetBoundVerts();

            bool transitionReady = false;
            int i = 0;
            int iEnd = obstacleBoundVerts.Length - 1;
            //Wait for first away -> towards transition
            while (i < obstacleBoundVerts.Length)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[(i + 1) % obstacleBoundVerts.Length]);
                if (v1.x < v2.x)
                {
                    if (transitionReady)
                    {
                        iEnd = i - 1;
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

            LinkedList<Vector2> contiguousVertices = new LinkedList<Vector2>();
            while (i != iEnd)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                i = (i + 1) % obstacleBoundVerts.Length;
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[i]);

                bool outOfBounds = (v1.x < lims[0].x && v2.x < lims[0].x) || (v1.x > lims[1].x && v2.x > lims[1].x) ||
                    (Det(lims[1] - lims[0], v1 - lims[0]) < 0 && Det(lims[1] - lims[0], v2 - lims[0]) < 0);
                if (v1.x < v2.x && !outOfBounds)
                {
                    
                    float t;
                    Vector2 dir = (v2 - v1).normalized;
                    if (IntersectLines2D(v1, dir, lims[0], (lims[1] - lims[0]).normalized, out t) &&
                        !(Det(lims[1] - lims[0], v1 - lims[0]) > 0 && Det(lims[1] - lims[0], v2 - lims[0]) > 0))
                    {
                        if (Det(lims[1] - lims[0], v1 - lims[0]) > 0)
                        {
                            v2 = v1 + t * dir;
                        }
                        else
                        {
                            v1 = v1 + t * dir;
                        }
                    }
                    

                    if (v1.x < lims[0].x && v2.x >= lims[0].x)
                    {
                        LineSegment ls = new LineSegment(v1, v2);
                        v1 = ls.p1 + ((lims[0].x - ls.p1.x) / ls.dir.x) * ls.dir;
                        v1 = new Vector2(lims[0].x - EPSILON, v1.y);
                    }

                    if (v1.x < lims[1].x && v2.x >= lims[1].x)
                    {
                        LineSegment ls = new LineSegment(v1, v2);
                        v2 = ls.p1 + ((lims[1].x - ls.p1.x) / ls.dir.x) * ls.dir;
                        //This ensures that beam will end on edges closer to source
                        v2 = new Vector2(lims[1].x + (1.0f - (v2.y / beamLength)) * EPSILON, v2.y);
                    }

                    if (transitionReady)
                    {
                        contiguousVertices = new LinkedList<Vector2>();
                        sortedKeyVertices.Add(contiguousVertices.AddLast(v1));
                        transitionReady = false;
                    }

                    LinkedListNode<Vector2> vNode = contiguousVertices.AddLast(v2);
                    sortedKeyVertices.Add(vNode);
                    
                }
                else
                {
                    transitionReady = true;
                }
                
            }

        }

        LinkedList<Vector2> topLightBound = new LinkedList<Vector2>();
        Vector2 vts = new Vector2(lims[0].x, beamLength);
        Vector2 vte = new Vector2(lims[1].x, beamLength);
        LinkedListNode<Vector2> leftBound = topLightBound.AddLast(vts);
        LinkedListNode<Vector2> rightBound = topLightBound.AddLast(vte);
        sortedKeyVertices.Add(leftBound);
        sortedKeyVertices.Add(rightBound);

        //Order by increasing x and then increasing y
        sortedKeyVertices = sortedKeyVertices.OrderBy(node => node.Value.x).ThenBy(node => node.Value.y).ToList();

        List<LinkedListNode<Vector2>> activeEdges = new List<LinkedListNode<Vector2>>();
        List<Vector2> beamBounds = new List<Vector2>();
        beamBounds.Add(lims[0]);

        LinkedListNode<Vector2> curClosestEdge = null;
        
        foreach (LinkedListNode<Vector2> vertNode in sortedKeyVertices)
        {
            if (vertNode.Next != null)
            {
                //This is not an end node
                Vector2 vs = vertNode.Value;

                if(vertNode.Previous == null)
                {
                    activeEdges.Add(vertNode);
                }
                else
                {
                    int j = activeEdges.IndexOf(vertNode.Previous);
                    activeEdges[j] = vertNode;
                }

                
                //First edge vertex gets added immediately
                if (curClosestEdge == null)
                {
                    curClosestEdge = vertNode;
                    beamBounds.Add(vertNode.Value);
                    continue;
                }

                LinkedListNode<Vector2> prevClosestEdge = curClosestEdge;

                LineSegment lsPrev = new LineSegment(prevClosestEdge.Value, prevClosestEdge.Next.Value);
                Vector2 clipPrev = lsPrev.p1 + ((vs.x - lsPrev.p1.x) / lsPrev.dir.x) * lsPrev.dir;

                Vector2 closestVert = new Vector2(vs.x, Mathf.Infinity);
                foreach (LinkedListNode<Vector2> activeEdge in activeEdges)
                {
                    Vector2 activeEdgeStart = activeEdge.Value;
                    Vector2 activeEdgeEnd = activeEdge.Next.Value;
                    LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                    Vector2 clip = ls.p1 + ((vs.x - ls.p1.x) / ls.dir.x) * ls.dir;
                    if (clip.y < closestVert.y)
                    {
                        closestVert = clip;
                        curClosestEdge = activeEdge;
                    }
                }

                if(prevClosestEdge.Next != curClosestEdge && vertNode == curClosestEdge)
                {
                    beamBounds.Add(clipPrev);
                }
                if(vertNode == curClosestEdge)
                {
                    beamBounds.Add(closestVert);
                }
            }
            else
            {
                activeEdges.Remove(vertNode.Previous);
                if(vertNode.Previous == curClosestEdge)
                {
                    //This is an end node
                    Vector2 ve = vertNode.Value;
                    beamBounds.Add(ve);

                    if (activeEdges.Count == 0)
                    {
                        //No more edges
                        break;
                    }
                    //Check if this is the end node of the currently closest edge
                    Vector2 nextClosestVert = new Vector2(ve.x, Mathf.Infinity);
                    foreach (LinkedListNode<Vector2> activeEdge in activeEdges)
                    {
                        Vector2 activeEdgeStart = activeEdge.Value;
                        Vector2 activeEdgeEnd = activeEdge.Next.Value;
                        LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                        Vector2 clip = ls.p1 + ((ve.x - ls.p1.x) / ls.dir.x) * ls.dir;
                        if (clip.y < nextClosestVert.y)
                        {
                            nextClosestVert = clip;
                            curClosestEdge = activeEdge;
                        }
                    }
                    beamBounds.Add(nextClosestVert);
                }             
                
            }
        }

        beamBounds.Add(lims[1]);

        return new List<Vector2>[] { beamBounds };
    }
    */


}
