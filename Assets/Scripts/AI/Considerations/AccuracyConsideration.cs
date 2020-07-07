using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class AccuracyConsideration : UtilityConsideration
{
    public AccuracyConsideration(UtilityRank baseRank) : base(baseRank)
    {

    }

    public override bool Score(Enemy me, out float weight)
    {
        Vector2 target = me.GetShootingTarget();
        //Check if AI has gun equipped
        IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm != null)
        {
            //Desire to shoot based on how close AI is aiming at player
            Transform barrelExit = firearm.GetBarrelExit();
            Plane2D los = new Plane2D(Vector2.Perpendicular(barrelExit.up), barrelExit.position);
            float aimError = Mathf.Min(los.DistanceToPoint(target) / 0.5f, 1.0f);
            weight = 1.0f - aimError;
            return true;
        }

        weight = 0.0f;
        return false;

    }

}
