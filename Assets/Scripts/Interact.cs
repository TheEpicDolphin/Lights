using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    List<Item> items = new List<Item>();
    Transform hand;

    // Start is called before the first frame update
    void Start()
    {
        hand = transform.parent.GetChild(1);   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(items.Count > 0)
            {
                if (hand.childCount > 0)
                {
                    Transform equipped = hand.GetChild(0);
                    equipped.parent = null;
                    equipped.gameObject.layer = 15;
                }

                Item closestItem = items[0];
                float closestDist = Vector3.Distance(closestItem.transform.position, transform.position);
                foreach(Item item in items)
                {
                    float newDist = Vector3.Distance(closestItem.transform.position, transform.position);
                    if (newDist < closestDist)
                    {
                        closestDist = newDist;
                        closestItem = item;
                    }
                }
                //Attach item to hand
                closestItem.transform.parent = hand;
                closestItem.transform.localPosition = Vector3.zero;
                closestItem.transform.localRotation = Quaternion.identity;
                items.Remove(closestItem);
                closestItem.gameObject.layer = 16;
            }
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Item>())
        {
            Item item = collision.GetComponent<Item>();
            if (!items.Contains(item))
            {
                items.Add(item);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Item>())
        {
            Item item = collision.GetComponent<Item>();
            if (items.Contains(item))
            {
                items.Remove(item);
                
            }
        }
    }

}
