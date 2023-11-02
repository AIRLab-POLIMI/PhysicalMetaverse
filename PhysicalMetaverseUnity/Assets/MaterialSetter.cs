using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
    [ContextMenu("Update textures")]
    // Start is called before the first frame update
    void Start()
    {
        //set this material to all children
        foreach (Transform child in transform)
        {
            //use renderer.sharedMaterial
            child.GetComponent<Renderer>().sharedMaterial = gameObject.GetComponent<Renderer>().sharedMaterial;
        }
    }
}
