using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Obstacle : MonoBehaviour
{
    EdgeCollider2D edgeCol;
    // Start is called before the first frame update
    void Start()
    {
        edgeCol = GetComponent<EdgeCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {

    }

    public Vector2[] GetBoundVerts()
    {
        Vector2[] verts = edgeCol.points;
        return verts;
    }

    public List<Projection1D> GetRelativeProjections(Transform beamTrans)
    {
        Vector3 relBeamDir = beamTrans.InverseTransformDirection(beamTrans.forward);
        Vector2 relBeamDir2d = new Vector2(relBeamDir.x, relBeamDir.z).normalized;

        List<Projection1D> projections = new List<Projection1D>();
        List<Vector2> connectedVerts = new List<Vector2>();
        int vertCount = crossSection.verts.Count;
        for (int i = 1; i < vertCount + 1; i++)
        {
            Vector3 curVert = new Vector3(crossSection.verts[i % vertCount].x, 0.0f, crossSection.verts[i % vertCount].y);
            Vector3 prevVert = new Vector3(crossSection.verts[i - 1].x, 0.0f, crossSection.verts[i - 1].y);

            Vector3 relCurVert = beamTrans.InverseTransformPoint(curVert);
            Vector3 relPrevVert = beamTrans.InverseTransformPoint(prevVert);

            Vector3 dir = (relCurVert - relPrevVert).normalized;
            Vector2 nRel2d = Vector2.Perpendicular(new Vector2(dir.x, dir.z)).normalized;
            if (Vector2.Dot(nRel2d, relBeamDir2d) > 0)
            {
                connectedVerts.Add(curVert);
            }
            else if (connectedVerts.Count > 0)
            {
                projections.Add(new Projection1D(connectedVerts));
                connectedVerts = new List<Vector2>();
            }
        }
        return projections;
    }

}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projection1D
{
    public List<Vector2> verts;

    public Projection1D(List<Vector2> verts)
    {
        this.verts = verts;
    }

    public Vector2 LeftBound()
    {
        return verts[0];
    }

    public Vector2 RightBound()
    {
        return verts[verts.Count - 1];
    }
}

public class Obstacle : MonoBehaviour
{
    Polygon crossSection;
    Rigidbody rb;
    
    int staticLayerMask = 10;
    int movingLayerMask = 11;
    public bool isMoving;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            rb.velocity += -Vector3.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rb.velocity += Vector3.right;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            rb.velocity += Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rb.velocity += -Vector3.forward;
        }
    }

    private void FixedUpdate()
    {
        isMoving = !(rb.velocity == Vector3.zero);
        gameObject.layer = isMoving ? movingLayerMask : staticLayerMask;
    }

    public List<Projection1D> GetRelativeProjections(Transform beamTrans)
    {
        Vector3 relBeamDir = beamTrans.InverseTransformDirection(beamTrans.forward);
        Vector2 relBeamDir2d = new Vector2(relBeamDir.x, relBeamDir.z).normalized;

        List<Projection1D> projections = new List<Projection1D>();
        List<Vector2> connectedVerts = new List<Vector2>();
        int vertCount = crossSection.verts.Count;
        for (int i = 1; i < vertCount + 1; i++)
        {
            Vector3 curVert = new Vector3(crossSection.verts[i % vertCount].x, 0.0f, crossSection.verts[i % vertCount].y);
            Vector3 prevVert = new Vector3(crossSection.verts[i - 1].x, 0.0f, crossSection.verts[i - 1].y);

            Vector3 relCurVert = beamTrans.InverseTransformPoint(curVert);
            Vector3 relPrevVert = beamTrans.InverseTransformPoint(prevVert);

            Vector3 dir = (relCurVert - relPrevVert).normalized;
            Vector2 nRel2d = Vector2.Perpendicular(new Vector2(dir.x, dir.z)).normalized;
            if(Vector2.Dot(nRel2d, relBeamDir2d) > 0)
            {
                connectedVerts.Add(curVert);
            }
            else if (connectedVerts.Count > 0)
            {
                projections.Add(new Projection1D(connectedVerts));
                connectedVerts = new List<Vector2>();
            }
        }
        return projections;
    }

}
*/
