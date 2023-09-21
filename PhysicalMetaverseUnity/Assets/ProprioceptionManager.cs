using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProprioceptionManager : MonoBehaviour
{
    //WARNING: AS OF NOW IF A STATION IS DISABLED IT WILL REMAIN IN THE LIST OF CURRENT STATIONS

    // Start is called before the first frame update
    void Start()
    {
        //log proprioception started
        Debug.Log("Proprioception started");

    }

    // Update is called once per frame
    void Update()
    {

    }

    //list current station
    public List<GameObject> _currentStations = new List<GameObject>();
    //on trigger stay with object tagged Station log
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Station"))
        {
            //add if not present
            if (!_currentStations.Contains(other.gameObject.transform.parent.gameObject))
            {
                _currentStations.Add(other.gameObject.transform.parent.gameObject);
            }
            Debug.Log(other.gameObject.transform.parent.gameObject.name + "entered");
            StationEnterEvent();
            
        }
    }

    //on trigger exit with object tagged Station log
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Station"))
        {
            _currentStations.Remove(other.gameObject.transform.parent.gameObject);
            Debug.Log(other.gameObject.transform.parent.gameObject.name + "exited");
        }
    }

    private void StationEnterEvent()
    {
        //key is (char)195
        NetworkingManager.Instance.Send(new byte[]{0x01}, 0xc3);
    }

}
