using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GeometryUtils;
using AlgorithmUtils;

public struct Tuple<T, V>
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

    MeshFilter meshFilt;
    const float EPSILON = 1e-5f;
    public float beamLength = 20.0f;
    public Vector2[] sourceLims = new Vector2[] { new Vector2(-0.5f, 0.0f), new Vector2(0.5f, 0.0f) };

    List<List<Vector2>> beamComponents = new List<List<Vector2>>();

    public Color beamColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
    Obstacle dummy;

    // Start is called before the first frame update
    void Start()
    {
        meshFilt = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRend = gameObject.AddComponent<MeshRenderer>();
        meshRend.material = new Material(Shader.Find("Custom/BeamShader"));
        //meshRend.material = new Material(Shader.Find("Standard"));
        meshRend.material.color = beamColor;

        /*
        Vector3 n = new Vector3(-1, 0, 0).normalized;
        Vector3 p0 = new Vector3(0, 0, 0);

        Matrix4x4 objectTransform = Matrix4x4.identity;
        objectTransform.SetColumn(0, new Vector3(1, 0, 0));
        objectTransform.SetColumn(1, new Vector3(0, 1, 0));
        objectTransform.SetColumn(2, new Vector3(0, 0, 1));
        objectTransform.SetColumn(3, new Vector4(-1, 1, 0, 1));

        Debug.Log("REFLECTION");
        Debug.Log(objectTransform);
        Debug.Log(Geometry.ReflectTransformAcrossPlane(n, p0, objectTransform));
        */
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indicesList = new List<int>();


        foreach (List<Vector2> beamComponent in beamComponents)
        {
            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(beamComponent.ToArray());
            int[] indices = tr.Triangulate();
            for (int i = 0; i < indices.Length; i++)
            {
                indicesList.Add(indices[i] + vertices.Count);
            }

            //Debug.Log("COMPONENT");
            for (int i = 0; i < beamComponent.Count; i++)
            {
                //Debug.Log(beamComponent[i].ToString("F4"));
                vertices.Add(new Vector3(beamComponent[i].x, beamComponent[i].y, 0));
            }
            //Debug.Log("----------------");

        }

        meshFilt.mesh.Clear();
        meshFilt.mesh.SetVertices(vertices);
        meshFilt.mesh.SetTriangles(indicesList.ToArray(), 0);
        meshFilt.mesh.RecalculateNormals();
        meshFilt.mesh.RecalculateBounds();


    }

    private void FixedUpdate()
    {
        Vector2 beamOrigin = transform.TransformPoint((sourceLims[0] + sourceLims[1]) / 2);
        Vector2 beamDir = transform.TransformDirection(new Vector2(0, 1));
        List<Obstacle> obstacles = GetObstaclesInBeam(beamOrigin, beamDir, 
                                    (sourceLims[1] - sourceLims[0]).magnitude / 2, beamLength);
        beamComponents = new List<List<Vector2>>();

        Cast(sourceLims, obstacles, Matrix4x4.identity, beamLength, 1, ref beamComponents);

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

    public List<Obstacle> GetObstaclesInBeam(Vector2 originWorld, Vector2 dirWorld, float beamWidth, float beamLength, Obstacle ignore = null)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(originWorld, beamWidth, dirWorld, beamLength, (1 << 12));
        List<Obstacle> obstacles = new List<Obstacle>();
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.GetComponent<Obstacle>())
            {
                Obstacle obstacle = hit.collider.GetComponent<Obstacle>();
                if(obstacle != ignore)
                {
                    obstacles.Add(obstacle);
                }
                
            }
        }
        return obstacles;
    }

    //TODO: add argument for rightHanded/leftHanded coordinate system for appropriate reversing of vertices
    public void Cast(Vector2[] lims, List<Obstacle> obstacles, Matrix4x4 beamLocalToCur, float beamLength, int maxRecurse, ref List<List<Vector2>> beamComponents)
    {
        Matrix4x4 curToBeamLocal = beamLocalToCur.inverse;
        Matrix4x4 worldToCur = beamLocalToCur * transform.worldToLocalMatrix;

        //Vector3 o = transform.TransformPoint(beamLocalToCur.MultiplyPoint3x4(Vector3.zero));
        //Debug.DrawLine(Vector3.zero, o, Color.cyan);

        Vector3 right = curToBeamLocal.GetColumn(0);
        Vector3 up = curToBeamLocal.GetColumn(1);
        Vector3 forward = curToBeamLocal.GetColumn(2);
        bool rightHandedCoords = Vector3.Dot(Vector3.Cross(right, up), forward) < 0;

        List<LinkedListNode<ObstacleVertex>> sortedKeyVertices = new List<LinkedListNode<ObstacleVertex>>();

        foreach (Obstacle obstacle in obstacles)
        {

            Vector2[] obstacleBoundVerts = obstacle.GetLocalBoundVerts(worldToCur, rightHandedCoords);

            bool transitionReady = false;
            int i = 0;
            //Wait for first away -> towards transition
            while (i < obstacleBoundVerts.Length)
            {
                Vector2 v1 = obstacleBoundVerts[i];
                Vector2 v2 = obstacleBoundVerts[(i + 1) % obstacleBoundVerts.Length];
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
                Vector2 v1 = obstacleBoundVerts[i];
                i = (i + 1) % obstacleBoundVerts.Length;
                Vector2 v2 = obstacleBoundVerts[i];

                
                bool isIntersectingSource = (Geometry.Det(lims[1] - lims[0], v1 - lims[0]) > 0) ^ (Geometry.Det(lims[1] - lims[0], v2 - lims[0]) > 0);
                Vector2 intersection;
                if (isIntersectingSource && Geometry.IntersectLines2D(lims[0], lims[1], v1, v2, out intersection))
                {
                    if (intersection.x > lims[0].x && intersection.x < lims[1].x)
                    {
                        return;
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


        LinkedList<ObstacleVertex> topLightBound = new LinkedList<ObstacleVertex>();
        Vector2 vts = new Vector2(lims[0].x - 0.1f, beamLength);
        Vector2 vte = new Vector2(lims[1].x + 0.1f, beamLength);
        sortedKeyVertices.Add(topLightBound.AddLast(new ObstacleVertex(vts, null)));
        sortedKeyVertices.Add(topLightBound.AddLast(new ObstacleVertex(vte, null)));

        //Order by increasing x and then increasing y
        sortedKeyVertices = sortedKeyVertices.OrderBy(node => node.Value.v.x).ThenBy(node => node.Value.v.y).ToList();

        List<LinkedListNode<ObstacleVertex>> activeEdges = new List<LinkedListNode<ObstacleVertex>>();
        List<Vector2> beamFunction = new List<Vector2>();
        List<ObstacleVertex> beamFunctionObj = new List<ObstacleVertex>();

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

                    beamFunctionObj.Add(new ObstacleVertex(clipPrev, prevClosestEdge.Value.obsRef));
                    beamFunctionObj.Add(new ObstacleVertex(closestVert, curClosestEdge.Value.obsRef));
                    //Reflection/refraction:
                    //prevClosestEdge.v -> clipPrev

                }
                else if (vertNode == curClosestEdge)
                {
                    //We are continuing a chain of connected edges
                    beamFunction.Add(closestVert);

                    beamFunctionObj.Add(new ObstacleVertex(closestVert, curClosestEdge.Value.obsRef));
                    //Reflection/refraction:
                    //prevClosestEdge.v -> vertNode.v

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
                            nextClosestVert.y = clip.y;
                            curClosestEdge = activeEdge;
                        }
                    }

                    beamFunction.Add(ve);
                    beamFunction.Add(nextClosestVert);

                    beamFunctionObj.Add(vertNode.Value);
                    beamFunctionObj.Add(new ObstacleVertex(nextClosestVert, curClosestEdge.Value.obsRef));
                    //Reflection/refraction:
                    //vertNode.v -> curClosestEdge.v

                }
            }

        }

        List<float> beamFunctionXs = new List<float>();
        foreach(ObstacleVertex p in beamFunctionObj)
        {
            beamFunctionXs.Add(p.v.x);
        }

        List<Vector2> beamComponent = new List<Vector2>();
        List<Tuple<Obstacle, Vector2[]>> illuminatedEdges = new List<Tuple<Obstacle, Vector2[]>>();

        //Binary search for left and right bounds (demarcations[i] and demarcations[i + 1])
        int s = Algorithm.BinarySearch(beamFunctionXs, CompCondition.LARGEST_LEQUAL, lims[0].x);
        int e = Algorithm.BinarySearch(beamFunctionXs, CompCondition.SMALLEST_GEQUAL, lims[1].x);

        Vector2 dir1 = (beamFunctionObj[s + 1].v - beamFunctionObj[s].v).normalized;
        Vector2 clipStart = beamFunctionObj[s].v + ((lims[0].x - beamFunctionObj[s].v.x) / dir1.x) * dir1;

        Vector2 dir2 = (beamFunctionObj[e].v - beamFunctionObj[e - 1].v).normalized;
        Vector2 clipEnd = beamFunctionObj[e - 1].v + ((lims[1].x - beamFunctionObj[e - 1].v.x) / dir2.x) * dir2;

        //Vector3 o = transform.TransformPoint(curToBeamLocal.MultiplyPoint3x4(Vector3.zero));
        //Debug.DrawLine(Vector3.zero, o, Color.cyan);
        //Vector3 o = transform.TransformPoint(beamLocalToCur.MultiplyPoint3x4(Vector3.zero));
        //Debug.DrawLine(Vector3.zero, o, Color.cyan);

        if (!(lims[0] == clipStart))
        {
            beamComponent.Add(curToBeamLocal.MultiplyPoint3x4(lims[0]));
        }
        beamComponent.Add(curToBeamLocal.MultiplyPoint3x4(clipStart));

        for (int j = s + 1; j < e; j++)
        {
            Obstacle obs = beamFunctionObj[j].obsRef;
            Vector2 v = curToBeamLocal.MultiplyPoint3x4(beamFunctionObj[j].v);
            Vector2 vLast = beamComponent.Last();

            if (!Mathf.Approximately(v.x, vLast.x))
            {
                illuminatedEdges.Add(new Tuple<Obstacle, Vector2[]>(obs, new Vector2[] { vLast, v }));
            }

            beamComponent.Add(v);
        }

        Obstacle lastObs = beamFunctionObj[e].obsRef;
        illuminatedEdges.Add(new Tuple<Obstacle, Vector2[]>(lastObs, 
                            new Vector2[] { beamComponent.Last(), curToBeamLocal.MultiplyPoint3x4(clipEnd) }));

        beamComponent.Add(curToBeamLocal.MultiplyPoint3x4(clipEnd));
        if (!(lims[1] == clipEnd))
        {
            beamComponent.Add(curToBeamLocal.MultiplyPoint3x4(lims[1]));
        }

        beamComponents.Add(beamComponent);

        if(maxRecurse == 0)
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
                obs.Cast(this, newLims, curToBeamLocal, beamLength, maxRecurse - 1, ref beamComponents);
            }

        }


    }

}
