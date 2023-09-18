using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndText : MonoBehaviour
{
    public GameObject text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //on trigger with player enable text
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            text.SetActive(true);
        }
    }
}
