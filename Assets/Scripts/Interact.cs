using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    List<GameObject> nearbyObjects = new List<GameObject>();
    Hand hand;

    // Start is called before the first frame update
    void Start()
    {
        hand = transform.parent.GetComponentInChildren<Hand>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(nearbyObjects.Count > 0)
            {
                if (hand.HasObjectEquipped())
                {
                    hand.UnequipObject();
                }

                GameObject closestItem = nearbyObjects[0];
                float closestDist = Vector3.Distance(closestItem.transform.position, transform.position);
                foreach(GameObject item in nearbyObjects)
                {
                    float newDist = Vector3.Distance(closestItem.transform.position, transform.position);
                    if (newDist < closestDist)
                    {
                        closestDist = newDist;
                        closestItem = item;
                    }
                }
                hand.EquipObject(closestItem);
                nearbyObjects.Remove(closestItem);
            }
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!nearbyObjects.Contains(collision.gameObject))
        {
            nearbyObjects.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (nearbyObjects.Contains(collision.gameObject))
        {
            nearbyObjects.Remove(collision.gameObject);

        }
    }

}
