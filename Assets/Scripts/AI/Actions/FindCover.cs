using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindCover : UtilityAction
{
    Player player;
    Enemy me;
    

    public FindCover(string name) : base(name)
    {
        
    }

    public override bool CheckPrerequisites(Dictionary<string, object> memory)
    {
        if (memory.ContainsKey("player"))
        {
            player = (Player)memory["player"];
        }
        else
        {
            return false;
        }

        if (memory.ContainsKey("me"))
        {
            me = (Enemy)memory["me"];
        }
        else
        {
            return false;
        }

        //Check if AI has gun equipped
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        if (firearm == null)
        {
            return false;
        }

        return true;
    }

    public override float Score(Dictionary<string, object> calculated)
    {
        //Desire is measured as value in range [0, 1], where 0 is low desire and 1 is high desire.

        //TODO: Desire to hide based on ammo remaining


        //Desire to hide based on proximity to target
        IFirearm firearm = player.hand?.GetEquippedObject()?.GetComponent<IFirearm>();
        float equippedFirearmRange = firearm.GetRange();
        float dist = Vector3.Distance(player.transform.position, me.transform.position);
        float proximity = 1 - Mathf.Max(equippedFirearmRange - dist, 0.0f) / equippedFirearmRange;


        float U = 1.0f;
        return U;
    }

    public override void Run(Dictionary<string, object> calculated)
    {

        Collider2D[] coverColliders = Physics2D.OverlapCircleAll(me.transform.position, 10.0f, 1 << 12);
        Vector2 playerDir = player.transform.position - me.transform.position;
        bool coverFound = false;
        Vector2 bestCoverLocation = Vector2.zero;
        float bestPreference = Mathf.NegativeInfinity;
        foreach(Collider2D coverCollider in coverColliders)
        {
            Cover cover = coverCollider.gameObject.GetComponent<Cover>();
            if (cover)
            {
                Vector2[] boundVerts = cover.GetWorldBoundVerts();
                for (int i = 0; i < boundVerts.Length; i++)
                {
                    Vector2 p1 = boundVerts[i];
                    Vector2 p2 = boundVerts[(i + 1) % boundVerts.Length];
                    Vector2 n = Vector2.Perpendicular(p2 - p1).normalized;
                    float dot = Vector2.Dot(-playerDir, n);
                    if(dot > 0)
                    {
                        float preference = dot + 1 / playerDir.magnitude;
                        if (preference > bestPreference)
                        {
                            bestPreference = preference;
                            bestCoverLocation = (p1 + p2) / 2 + 2.5f * n;
                        }
                    }
                    
                }
                
            }
        }

        if (coverFound)
        {
            me.NavigateToWhileAvoiding(bestCoverLocation, player.transform.position);
        }
        else
        {
            //TODO: Choose random position
            me.NavigateToWhileAvoiding(Vector2.zero, player.transform.position);
        }
        
    }
}
