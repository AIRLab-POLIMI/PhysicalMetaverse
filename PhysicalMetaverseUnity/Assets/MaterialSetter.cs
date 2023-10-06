using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //set this material to all children
        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material = gameObject.GetComponent<Renderer>().material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
