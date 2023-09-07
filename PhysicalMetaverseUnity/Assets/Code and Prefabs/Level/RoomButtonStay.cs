using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomButtonStay : MonoBehaviour
{
    //target door gameobject
    public GameObject door;
    public int peopleOnButton = 0;
    //initial door position
    public Vector3 _initialDoorPosition;
    //collider list
    public List<Collider> colliders = new List<Collider>();
    // Start is called before the first frame update
    void Start()
    {
        _initialDoorPosition = door.transform.position;
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
        door.transform.position = new Vector3(_initialDoorPosition.x, _initialDoorPosition.y + 1.5f, _initialDoorPosition.z);
        //call blink twice of who entered
        try{
            other.gameObject.GetComponentInChildren<Blink>().BlinkTwice();
        } catch {
            //do nothing
        }
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
                door.transform.position = new Vector3(_initialDoorPosition.x, _initialDoorPosition.y, _initialDoorPosition.z);
            } catch {
                //do nothing
            }
        }
    }

}
