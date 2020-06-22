using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootFirearm : UtilityAction
{
    Enemy me;
    IFirearm firearm;
    public ShootFirearm()
    {
        
    }

    //Guaranteed to occur once
    public override float Score()
    {
        return 0.0f;
    }

    public override void Run()
    {
        me.Attack();
    }
}
