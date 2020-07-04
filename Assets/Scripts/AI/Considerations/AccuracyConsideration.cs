using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtils;

public class AccuracyConsideration : UtilityConsideration
{
    public AccuracyConsideration(int rank) : base(rank)
    {

    }

    public override bool Score(Dictionary<string, object> memory, out float weight)
    {
        if (memory.ContainsKey("shooting_target") && memory.ContainsKey("me"))
        {
            Vector2 target = (Vector2)memory["shooting_target"];
            float radius = (float)memory["target_radius"];
            Enemy me = (Enemy)memory["me"];
            //Check if AI has gun equipped
            IFirearm firearm = me.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
            if(firearm != null)
            {
                //Desire to shoot based on how close AI is aiming at player
                Transform barrelExit = firearm.GetBarrelExit();
                Plane2D los = new Plane2D(Vector2.Perpendicular(barrelExit.up), barrelExit.position);
                float aimError = Mathf.Min(los.DistanceToPoint(target) / (1.5f * radius), 1.0f);

                weight = aimError;
                return true;
            }
        }
        weight = 0.0f;
        return false;

    }

}
