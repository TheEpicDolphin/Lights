﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tuple<T, V> : System.IComparable<T> where T : System.IComparable<T>
{
    public T el1;
    public V el2;

    public Tuple(T el1, V el2)
    {
        this.el1 = el1;
        this.el2 = el2;
    }

    public int CompareTo(T x)
    {
        return this.el1.CompareTo(x);
    }

}

public class Beam : MonoBehaviour
{

    Vector3 bottomLeftOrigin;
    Vector3 topLeftOrigin;
    Vector3 topRightOrigin;
    Vector3 bottomRightOrigin;

    public List<Obstacle> obstacles;

    public ObstacleDetector obstacleDetector;

    MeshFilter meshFilt;
    // Start is called before the first frame update
    void Start()
    {

        meshFilt = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRend = gameObject.AddComponent<MeshRenderer>();
        meshRend.material = new Material(Shader.Find("Custom/BeamShader"));
        meshRend.material.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);


        Debug.DrawRay(transform.position, 10.0f * transform.forward, Color.magenta, 5.0f);
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
        

    }

    private void Cast()
    {
        List<Tuple<float, LinkedListNode<Vector2>>> sortedKeyVertices = new List<Tuple<float, LinkedListNode<Vector2>>>();
        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleBoundVerts = obstacle.GetBoundVerts();

            bool transitionReady = false;
            int i = 0;
            int iEnd = 0;
            //Wait for first away -> towards transition
            while (i < obstacleBoundVerts.Length)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i % obstacleBoundVerts.Length]);
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

            LinkedList<Vector2> contiguousVertices = new LinkedList<Vector2>();
            i = iEnd + 1;
            while (i != iEnd)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                i = (i + 1) % obstacleBoundVerts.Length;
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                if (v1.x < v2.x)
                {
                    LinkedListNode<Vector2> vNode = contiguousVertices.AddLast(v1);
                    sortedKeyVertices.Add(new Tuple<float, LinkedListNode<Vector2>>(v1.x, vNode));
                    transitionReady = true;
                }
                else if (transitionReady)
                {
                    LinkedListNode<Vector2> vNode = contiguousVertices.AddLast(v1);
                    sortedKeyVertices.Add(new Tuple<float, LinkedListNode<Vector2>>(v1.x, vNode));
                    contiguousVertices = new LinkedList<Vector2>();
                    transitionReady = false;
                }
                
            }

        }
        sortedKeyVertices.Sort();

        List<LinkedListNode<Vector2>> activeEdges = new List<LinkedListNode<Vector2>>();
        List<Vector2> hull = new List<Vector2>();

        foreach (Tuple<float, LinkedListNode<Vector2>> keyVert in sortedKeyVertices)
        {
            LinkedListNode<Vector2> vertNode = keyVert.el2;

            if (vertNode.Next != null)
            {
                //This is not an end node
                Vector2 vs = vertNode.Value;
                activeEdges.Add(vertNode);

                Vector2 closestVert = new Vector2(vs.x, Mathf.Infinity);
                foreach (LinkedListNode<Vector2> activeEdge in activeEdges)
                {
                    Vector2 activeEdgeVert = activeEdge.Value;
                    if (activeEdgeVert.y < closestVert.y)
                    {
                        closestVert = activeEdgeVert;
                    }
                }
                hull.Add(closestVert);
            }
            else
            {
                //This is an end node
                Vector2 ve = vertNode.Value;
                activeEdges.Remove(vertNode.Previous);

                Vector2 nextClosestVert = new Vector2(ve.x, Mathf.Infinity);
                foreach (LinkedListNode<Vector2> activeEdge in activeEdges)
                {
                    Vector2 activeEdgeStart = activeEdge.Value;
                    Vector2 activeEdgeEnd = activeEdge.Next.Value;
                    LineSegment ls = new LineSegment(activeEdgeStart, activeEdgeEnd);
                    Vector2 proj = ls.p1 + (ve.x - ls.p1.x) * ls.dir;
                    if (proj.y < nextClosestVert.y)
                    {
                        nextClosestVert = proj;
                    }
                }

                hull.Add(nextClosestVert);
            }
        }
    }

    /*
    private void Cast()
    {
        //Starting vert index maps to ending vert index
        List<int> edgeMap = new List<int>();
        List<Vector2> verts = new List<Vector2>();
        List<Tuple<float, int>> sortedVertInds = new List<Tuple<float, int>>();
        foreach (Obstacle obstacle in obstacles)
        {
            Vector2[] obstacleBoundVerts = obstacle.GetBoundVerts();
            for (int i = 0; i < obstacleBoundVerts.Length - 1; i++)
            {
                Vector2 v1 = transform.InverseTransformPoint(obstacleBoundVerts[i]);
                Vector2 v2 = transform.InverseTransformPoint(obstacleBoundVerts[i + 1]);

                edgeMap[verts.Count] = verts.Count + 1;
                if (v1.x < v2.x)
                {
                    verts.Add(v1);
                    verts.Add(v2);
                    sortedVertInds.Add(new Tuple<float, int>(v1.x, verts.Count));
                }
                else
                {
                    verts.Add(v2);
                    verts.Add(v1);
                    sortedVertInds.Add(new Tuple<float, int>(v2.x, verts.Count));
                }

            }
        }
        sortedVertInds.Sort();

        List<LineSegment> activeEdges = new List<LineSegment>();
        List<Vector2> hull = new List<Vector2>();

        foreach (Tuple<float, int> keyVertInd in sortedVertInds)
        {
            int vi = keyVertInd.el2;
            
            
            if (edgeMap.Contains(vi))
            {
                Vector2 vs = verts[vi];
                Vector2 ve = verts[edgeMap[vi]];
                activeEdges.Add(new LineSegment(vs, ve));


                //START point

                hull.Add(closestVert);
            }
            else
            {
                //END point
                Vector2 ve = verts[vi];

                Vector2 nextVert = new Vector2(ve.x, Mathf.Infinity);
                foreach (LineSegment ls in activeEdges)
                {
                    Vector2 proj = ls.p1 + (ve.x - ls.p1.x) * ls.dir;
                    if(proj.y < nextVert.y)
                    {
                        proj = nextVert;
                    }
                }
                activeEdges.Remove();
                hull.Add(nextVert);
            }
        }
    }
    */

}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class LightRay
{
    float t;
    float maxT;
    bool onStatic;
    Obstacle currentObstructor;
    LightRayState state;
    public enum LightRayState
    {
        Initial,
        ObstructedByStatic,
        ObstructedByMoving
    }

    public LightRay(float maxT)
    {
        this.t = maxT;
        this.maxT = maxT;

        this.onStatic = false;
        this.currentObstructor = null;
        state = LightRayState.Initial;
    }

    public Vector3[] Cast(Vector3 origin, Vector3 dir, List<Obstacle> movingObstacles, List<Obstacle> staticObstacles)
    {
        Ray lray = new Ray(origin, dir);
        RaycastHit hit;

        switch (state)
        {
            case LightRayState.Initial:
                foreach (Obstacle movingObstacle in movingObstacles)
                {
                    Collider obstacleCollider = movingObstacle.GetComponent<Collider>();
                    if (obstacleCollider.Raycast(lray, out hit, t))
                    {
                        if (hit.distance < t)
                        {
                            currentObstructor = movingObstacle;
                            t = hit.distance;
                            state = LightRayState.ObstructedByMoving;
                        }
                    }
                }

                foreach (Obstacle staticObstacle in staticObstacles)
                {
                    Collider obstacleCollider = staticObstacle.GetComponent<Collider>();
                    if (obstacleCollider.Raycast(lray, out hit, t))
                    {
                        if (hit.distance < t)
                        {
                            currentObstructor = staticObstacle;
                            t = hit.distance;
                            state = LightRayState.ObstructedByStatic;
                        }
                    }
                }

                break;
            case LightRayState.ObstructedByStatic:
                if (this.currentObstructor != null && currentObstructor.isMoving)
                {
                    t = maxT;
                    state = LightRayState.ObstructedByMoving;
                }

                foreach (Obstacle movingObstacle in movingObstacles)
                {
                    Collider obstacleCollider = movingObstacle.GetComponent<Collider>();
                    if (obstacleCollider.Raycast(lray, out hit, t))
                    {
                        if (hit.distance < t)
                        {
                            currentObstructor = movingObstacle;
                            t = hit.distance;
                            state = LightRayState.ObstructedByMoving;
                        }
                    }
                }

                break;
            case LightRayState.ObstructedByMoving:
                t = maxT;

                foreach (Obstacle movingObstacle in movingObstacles)
                {
                    Collider obstacleCollider = movingObstacle.GetComponent<Collider>();
                    if (obstacleCollider.Raycast(lray, out hit, t))
                    {
                        if (hit.distance < t)
                        {
                            currentObstructor = movingObstacle;
                            t = hit.distance;
                        }
                    }
                }

                foreach (Obstacle staticObstacle in staticObstacles)
                {
                    Collider obstacleCollider = staticObstacle.GetComponent<Collider>();
                    if (obstacleCollider.Raycast(lray, out hit, t))
                    {
                        if (hit.distance < t)
                        {
                            currentObstructor = staticObstacle;
                            t = hit.distance;
                            state = LightRayState.ObstructedByStatic;
                        }
                    }
                }
                break;
            default:
                //Shouldnt reach here
                break;
        }

        Debug.DrawRay(origin, t * dir, Color.cyan);

        //Multiple intersection points would signify reflections/filters/refractions
        return new Vector3[] { t };
    }
}

public class Beam : MonoBehaviour
{
    
    Vector3 bottomLeftOrigin;
    Vector3 topLeftOrigin;
    Vector3 topRightOrigin;
    Vector3 bottomRightOrigin;

    List<Obstacle> obstructors;

    List<Obstacle> staticObstacles;
    List<Obstacle> movingObstacles;

    public ObstacleDetector staticDetector;
    public ObstacleDetector movingDetector;

    const int NUM_RAYS_X = 10;
    const int NUM_RAYS_Y = 10;
    const float WIDTH = 2.0f;
    const float HEIGHT = 2.0f;
    LightRay[,] lightRays = new LightRay[NUM_RAYS_Y, NUM_RAYS_X];

    MeshFilter meshFilt;
    // Start is called before the first frame update
    void Start()
    {

        meshFilt = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRend = gameObject.AddComponent<MeshRenderer>();
        meshRend.material = new Material(Shader.Find("Custom/BeamShader"));
        meshRend.material.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);

        meshFilt.mesh = new Mesh();
        meshFilt.mesh.vertices = new Vector3[NUM_RAYS_X * NUM_RAYS_Y * 4];
        meshFilt.mesh.triangles = new int[NUM_RAYS_X * NUM_RAYS_Y * 2 * 5];
        

        Debug.DrawRay(transform.position, 10.0f * transform.forward, Color.magenta, 5.0f);

        for (int i = 0; i < NUM_RAYS_Y; i++)
        {
            for (int j = 0; j < NUM_RAYS_X; j++)
            {
                lightRays[i, j] = new LightRay(50.0f);
            }
        }
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
        meshFilt.mesh.Clear();
        Vector3[] verts = new Vector3[2 * (NUM_RAYS_X + 1) * (NUM_RAYS_Y + 1)];
        int offset = (NUM_RAYS_X + 1) * (NUM_RAYS_Y + 1);

        for (int i = 0; i < NUM_RAYS_Y + 1; i++)
        {
            for (int j = 0; j < NUM_RAYS_X + 1; j++)
            {
                float dy = (HEIGHT / NUM_RAYS_Y) * (i - NUM_RAYS_Y / 2.0f);
                float dx = (WIDTH / NUM_RAYS_X) * (j - NUM_RAYS_X / 2.0f);
                verts[j + i * NUM_RAYS_X] = new Vector3(dx, dy, 0.0f);
                verts[(j + i * NUM_RAYS_X) + offset] = new Vector3(dx, dy, 50.0f);
            }
        }

        for (int i = 0; i < NUM_RAYS_Y; i++)
        {
            for(int j = 0; j < NUM_RAYS_X; j++)
            {
                float dy_c = (HEIGHT / NUM_RAYS_Y) * (i - (NUM_RAYS_Y - 1) / 2.0f);
                float dx_c = (WIDTH / NUM_RAYS_X) * (j - (NUM_RAYS_X - 1) / 2.0f);
                float[] depths = lightRays[i, j].Cast(transform.position + dy_c * transform.up + dx_c * transform.right, transform.forward, 
                                    movingDetector.obstacles, staticDetector.obstacles);

                verts[(j + i * NUM_RAYS_X) + offset].z = depths[0];


                float z_current = verts[(j + i * NUM_RAYS_X) + offset].z;
                if (i > 0)
                {
                    float z_below = verts[(j + (i - 1) * NUM_RAYS_X) + offset].z;
                    if (z_below > )
                }

                if(j > 0)
                {
                    float z_left = verts[(j + i * NUM_RAYS_X) + offset].z;
                }
            }
        }
        
    }
}
*/
