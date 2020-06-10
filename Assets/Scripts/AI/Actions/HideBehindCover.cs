using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideBehindCover : UtilityAction
{
    //Use exposure time as measure of how much danger the enemy feels it is in

    public HideBehindCover(string name) : base(name)
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
