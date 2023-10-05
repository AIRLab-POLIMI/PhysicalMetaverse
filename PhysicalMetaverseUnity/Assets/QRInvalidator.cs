using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRInvalidator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //on trigger stay with object tagged Station log
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Station"))
        {
            //call InvalidateStation of the station
            try{
                //log invalidating station
                //Debug.Log("Invalidating station");
                //if not tracked
                if (!other.gameObject.transform.parent.gameObject.GetComponent<SingleStationManager>()._tracked)
                    //invalidate station
                    other.gameObject.transform.parent.gameObject.GetComponent<SingleStationManager>().InvalidateStation();
            }
            catch (System.Exception e)
            {
            }
        }
    }
}
