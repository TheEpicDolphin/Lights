using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeekFromCover : UtilityAction
{
    //Use line of sight to figure out if enemy wants to peek to shoot player
    //Raycast

    public PeekFromCover(string name) : base(name)
    {
        
    }

    public override bool CheckPrerequisites(Dictionary<string, object> memory)
    {
        
        return true;
    }

    public override float Score(Dictionary<string, object> calculated)
    {
        return 0.0f;
    }

    public override void Run(Dictionary<string, object> calculated)
    {
        
    }
}
