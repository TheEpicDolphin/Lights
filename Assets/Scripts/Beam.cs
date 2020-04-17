using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tuple<T, V> : System.IComparable where T : System.IComparable<T>
{
    public T el1;
    public V el2;

    public Tuple(T el1, V el2)
    {
        this.el1 = el1;
        this.el2 = el2;
    }

    public int CompareTo(object x)
    {
        Tuple<T, V> tuple = x as Tuple<T, V>;
        return this.el1.CompareTo(tuple.el1);
    }

}

//Vector.up is forward in 2D

public class Beam : MonoBehaviour
{

    Vector3 bottomLeftOrigin;
    Vector3 topLeftOrigin;
    Vector3 topRightOrigin;
    Vector3 bottomRightOrigin;

    public List<Obstacle> obstacles = new List<Obstacle>();

    ObstacleDetector obstacleDetector;

    MeshFilter meshFilt;
    const float EPSILON = 1e-5f;
    public float beamLength = 20.0f;
    public float[] xLims = new float[] { -0.5f, 0.5f };

    public Color beamColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
    // Start is called before the first frame update
    void Start()
    {
        obstacleDetector = GetComponentInChildren<ObstacleDetector>();

        meshFilt = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRend = gameObject.AddComponent<MeshRenderer>();
        meshRend.material = new Material(Shader.Find("Custom/BeamShader"));
        //meshRend.material = new Material(Shader.Find("Standard"));
        meshRend.material.color = beamColor;

        
        //Debug.DrawRay(transform.position, 10.0f * transform.up, Color.magenta, 5.0f);
    }


    private Mesh CreateBeamMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[]
        {
            bottomLeftOrigin, topLeftOrigin, topRightOrigin, bottomRightOrigin,
            (bottomLeftOrigin + transform.forward), (topLeftOrigin + transform.forward), (topRightOrigin + transform.forward), (bottomRightOrigin + transform.forward)
        };

        mesh.triangles = new int[] { 4, 1, 0, 5, 2, 1, 6, 3, 2, 7, 0, 3,
                                     4, 5, 1, 5, 6, 2, 6, 7, 3, 7, 4, 0};

        mesh.RecalculateNormals();
        return mesh;
    }

    // Update is called once per frame
    void Update()
    {

        List<Vector2>[] beamBoundSections = Cast(xLims);
        List<Vector2> beamBounds = beamBoundSections[0];

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(beamBounds.ToArray());
        int[] indices = tr.Triangulate();

        //Debug.Log("START");
        // Create the Vector3 vertices
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < beamBounds.Count; i++)
        {
            //Debug.Log(beamBounds[i].ToString("F4"));
            vertices.Add(new Vector3(beamBounds[i].x, beamBounds[i].y, 0));
        }

        meshFilt.mesh.Clear();
        meshFilt.mesh.SetVertices(vertices);
        meshFilt.mesh.SetTriangles(indices, 0);
        meshFilt.mesh.RecalculateNormals();
        meshFilt.mesh.RecalculateBounds();
        
    }
    
    //Each array element is a separate beam bound. The array has length > 1 when there are color filters, mirrors,
    //or refractive crystals in the path of the light beam
    private List<Vector2>[] Cast(float[] xLims)
    {

        List<LinkedListNode<Vector2>> sortedKeyVertices = new List<LinkedListNode<Vector2>>();
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

                if (v1.x < v2.x && !((v1.x < xLims[0] && v2.x < xLims[0]) || (v1.x > xLims[1] && v2.x > xLims[1])))
                {
                    if (transitionReady)
                    {
                        if (v1.x < xLims[0] && v2.x >= xLims[0])
                        {
                            LineSegment ls = new LineSegment(v1, v2);
                            v1 = ls.p1 + ((xLims[0] - ls.p1.x) / ls.dir.x) * ls.dir;
                            v1 = new Vector2(xLims[0] - EPSILON, v1.y);
                        }
                        
                        contiguousVertices = new LinkedList<Vector2>();
                        sortedKeyVertices.Add(contiguousVertices.AddLast(v1));
                        transitionReady = false;
                    }

                    if (v1.x < xLims[1] && v2.x >= xLims[1])
                    {
                        LineSegment ls = new LineSegment(v1, v2);
                        v2 = ls.p1 + ((xLims[1] - ls.p1.x) / ls.dir.x) * ls.dir;
                        //This ensures that beam will end on edges closer to source
                        v2 = new Vector2(xLims[1] + (1.0f - (v2.y / beamLength)) * EPSILON, v2.y);
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
        Vector2 vts = new Vector2(xLims[0], beamLength);
        Vector2 vte = new Vector2(xLims[1], beamLength);
        LinkedListNode<Vector2> leftBound = topLightBound.AddLast(vts);
        LinkedListNode<Vector2> rightBound = topLightBound.AddLast(vte);
        sortedKeyVertices.Add(leftBound);
        sortedKeyVertices.Add(rightBound);

        //Order by increasing x and then increasing y
        sortedKeyVertices = sortedKeyVertices.OrderBy(node => node.Value.x).ThenBy(node => node.Value.y).ToList();

        List<LinkedListNode<Vector2>> activeEdges = new List<LinkedListNode<Vector2>>();
        List<Vector2> beamBounds = new List<Vector2>();
        beamBounds.Add(xLims[0] * Vector2.right);

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

        beamBounds.Add(xLims[1] * Vector2.right);

        return new List<Vector2>[] { beamBounds };
    }


}
