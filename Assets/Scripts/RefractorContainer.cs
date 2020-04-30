using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RefractorContainer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Refractor refractor = GetComponentInChildren<Refractor>();
        Refractor inverseRefractor = Instantiate(refractor, transform);
        inverseRefractor.n2_n1 = 1.0f / refractor.n2_n1;
        EdgeCollider2D ec = refractor.GetComponent<EdgeCollider2D>();
        EdgeCollider2D ecInverse = inverseRefractor.GetComponent<EdgeCollider2D>();
        ecInverse.points = ec.points.Reverse().ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
