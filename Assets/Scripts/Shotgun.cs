using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Item
{
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
