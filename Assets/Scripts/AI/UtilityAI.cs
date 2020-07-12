using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;



public class UtilityAI
{
    public Dictionary<string, UtilityBucket> utilityMemory;
    [XmlElement("actions")]
    public List<UtilityAction> actions;

    public UtilityAI CreateFromXLM()
    {
        Hero hero = XMLOp.Deserialize<Hero>("hero.xml");
        Debug.Log(hero.name);
    }
    
}
