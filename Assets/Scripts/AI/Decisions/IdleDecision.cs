﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleDecision : UtilityDecision
{
    public IdleDecision(string name) : base(name)
    {

    }

    public override void Execute(Dictionary<string, object> memory)
    {
        //return new Wait(Random.Range(1.5f, 4.0f));
    }
}
