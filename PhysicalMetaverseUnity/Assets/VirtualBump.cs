using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualBump : MonoBehaviour
{
    public VirtualJetson _jetson;
    public bool _colliding = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }
    //if triggering with something not ground send bump
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "Ground")
        {
            _jetson.Send(new byte[]{0x01}, 0xf0);
            Debug.Log("BUMP");
            _colliding = true;
        }
    }

    //check trigger collision
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Ground")
        {
            _jetson.Send(new byte[]{0x00}, 0xf0);
            Debug.Log("END BUMP");
            _colliding = false;
        }
    }


}
