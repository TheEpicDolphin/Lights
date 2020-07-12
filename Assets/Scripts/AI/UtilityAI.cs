using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;


[XmlRoot("UtilityAI")]
public class UtilityAI
{
    private Dictionary<string, UtilityAction> map;

    [XmlElement("Nav", typeof(NavigateToStaticDestination))]
    [XmlElement("Aim", typeof(AimAtPlayer))]
    [XmlElement("Shoot", typeof(ShootAtPlayer))]
    [XmlElement("ExposeFromCover", typeof(ExposeFromCover))]
    [XmlElement("TakeCover", typeof(TakeCover))]
    [XmlElement("Strafe", typeof(Strafe))]
    public UtilityAction[] actions;

    public static UtilityAI CreateFromXML()
    {
        ;
        UtilityAI uai = XMLOp.Deserialize<UtilityAI>(
            Path.Combine(Application.dataPath, "uai.xml"));
        Debug.Log(uai.actions.Length);
        return uai;
    }
    
}
