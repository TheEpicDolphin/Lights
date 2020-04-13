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

    public float[] Cast(Vector3 origin, Vector3 dir, List<Obstacle> movingObstacles, List<Obstacle> staticObstacles)
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
        return new float[] { t };
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

    // Start is called before the first frame update
    void Start()
    {

        MeshFilter meshFilt = gameObject.AddComponent<MeshFilter>();
        meshFilt.mesh = CreateBeamMesh();
        MeshRenderer meshRend = gameObject.AddComponent<MeshRenderer>();
        meshRend.material = new Material(Shader.Find("Custom/BeamShader"));
        meshRend.material.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);

        

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
        //Debug.Log(transform.forward);
        //Debug.Log(transform.InverseTransformDirection(transform.forward));

        for (int i = 0; i < NUM_RAYS_Y; i++)
        {
            for(int j = 0; j < NUM_RAYS_X; j++)
            {
                float dy = (HEIGHT / NUM_RAYS_Y) * (i - (NUM_RAYS_Y - 1) / 2.0f);
                float dx = (WIDTH / NUM_RAYS_X) * (j - (NUM_RAYS_X - 1) / 2.0f);
                lightRays[i, j].Cast(transform.position + dy * transform.up + dx * transform.right, transform.forward, 
                                    movingDetector.obstacles, staticDetector.obstacles);
            }
        }
        
    }

    /*
    private void Cast()
    {
        List<Projection1D> projections = new List<Projection1D>();
        foreach (Obstacle obstacle in obstructors)
        {
            projections.AddRange(obstacle.GetRelativeProjections(transform));
        }

        List<Projection1D> sortedProjections = projections.OrderBy(projection => projection.LeftBound().x).ToList();

        MinHeap<int, Projection1D> zBufferQueue = new MinHeap<int, Projection1D>();

        int z = 0;
        List<Vector2> hull = new List<Vector2>();
        for(int i = 0; i < sortedProjections.Count; i++)
        {
            Projection1D projection = sortedProjections[i];
            int u = i + 1;
            Projection1D nextProjection = sortedProjections[u];
            Vector2 nextBound = nextProjection.LeftBound();
            foreach (Vector2 vert in projection.verts)
            {
                if(vert.x > nextBound.x)
                {
                    if(vert.y > nextBound.y)
                    {
                        zBufferQueue.Insert(z + 1, nextProjection);
                    }
                    else
                    {
                        zBufferQueue.Insert(z - 1, nextProjection);
                    }
                    u += 1;
                    nextProjection = sortedProjections[u];
                    nextBound = nextProjection.LeftBound();
                }   
            }
        }
    }
    */

}
