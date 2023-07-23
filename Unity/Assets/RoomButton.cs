using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomButton : MonoBehaviour
{
    //target door gameobject
    public GameObject door;
    public GameObject button2;
    private bool notAlreadyBlinked = true;
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
        if (other.gameObject.tag == "Robot")
        {
            //destroy door
            Destroy(door);
        }
        //if present enable button2
        if (button2 != null)
        {
            button2.SetActive(true);
        }
        if (notAlreadyBlinked)
        {
            //call blink twice of who entered
            other.gameObject.GetComponentInChildren<Blink>().BlinkTwice();
            notAlreadyBlinked = false;
        }
    }
}
