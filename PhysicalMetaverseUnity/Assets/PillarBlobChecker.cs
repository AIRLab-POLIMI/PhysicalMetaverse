using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarBlobChecker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //color this gameobject in yellow
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    void OnTriggerStay(Collider other){
        //if colliding with "Station" color this gameobject in red
        if(other.gameObject.CompareTag("Station")){
            //change color
            GetComponent<Renderer>().material.color = Color.red;
        }
    }

    
}
