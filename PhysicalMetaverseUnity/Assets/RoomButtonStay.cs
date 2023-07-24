using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomButtonStay : MonoBehaviour
{
    //target door gameobject
    public GameObject door;
    public int peopleOnButton = 0;
    //collider list
    public List<Collider> colliders = new List<Collider>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    //on trigger of capsule collider disable door
    void OnTriggerEnter(Collider other)
    {
        //add collider if not present
        if (!colliders.Contains(other))
        {
            colliders.Add(other);
        }
        //if (other.gameObject.tag == "Player")
        //{
            //door.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.4f);
            //set door to trigger
            //door.GetComponent<BoxCollider>().isTrigger = true;
            //move up 1.5
        door.transform.position = new Vector3(door.transform.position.x, 3.0f, door.transform.position.z);
        //call blink twice of who entered
        other.gameObject.GetComponentInChildren<Blink>().BlinkTwice();
        //}
    }

    //ontriggerexit reenable
    void OnTriggerExit(Collider other)
    {
        //remove collider if present
        if (colliders.Contains(other))
        {
            colliders.Remove(other);
        }
        if (colliders.Count == 0)
        {
            try {
                //set door alpha to one
                //door.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
                //set door to not trigger
                //door.GetComponent<BoxCollider>().isTrigger = false;
                //move to y 1.5
                door.transform.position = new Vector3(door.transform.position.x, 1.5f, door.transform.position.z);
            } catch {
                //do nothing
            }
        }
    }

}
