using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarBlobChecker : MonoBehaviour
{
    public bool _blobEnabled = true;
    public bool _disableMesh = true;
    private bool _prevCollided = false;
    private LidarManager _lidarManager;
    public int _pillarId = 0;
    public int _stationId = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetPillarId(int id){
        _pillarId = id;
        //ad id to end of name
        gameObject.name += id.ToString();
    }

    public void SetLidarManager(LidarManager lidarManager){
        _lidarManager = lidarManager;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        _stationId = -1;
        _lidarManager.SetBlobAt(_pillarId, -1);
        /*
        //color this gameobject in yellow
        GetComponent<Renderer>().material.color = Color.yellow;
        if(!_prevCollided)
        {
            //enable mesh
            GetComponent<MeshRenderer>().enabled = true;
        }
        _prevCollided = false;*/
    }

    void OnTriggerStay(Collider other){
        if(!_blobEnabled){
            return;
        }
        //if colliding with "Station" color this gameobject in red
        if(other.gameObject.CompareTag("Station")){
            //get station id from last character of name of parent
            //if gameobject has a parent
            if(other.gameObject.transform.parent == null){
                return;
            }
            string stationName = other.gameObject.transform.parent.name;
            _stationId = int.Parse(stationName.Substring(stationName.Length - 1));
            _lidarManager.SetBlobAt(_pillarId, _stationId);
            /*
            //check name of the father, if ends with 1 color green, if 0 color red
            //change color
            if(other.gameObject.transform.parent.name.EndsWith("1")){
                //GetComponent<Renderer>().material.color = Color.blue;
                //disable mesh
                GetComponent<MeshRenderer>().enabled = !_disableMesh;
                _prevCollided = true;
            }else{
                //GetComponent<Renderer>().material.color = Color.red;
                //disable mesh
                GetComponent<MeshRenderer>().enabled = !_disableMesh;
                _prevCollided = true;
            }*/
        }
    }

    /*void OnTriggerExit(Collider other){
        //color this gameobject in yellow
        GetComponent<Renderer>().material.color = Color.yellow;
        //enable mesh
        GetComponent<MeshRenderer>().enabled = true;
    }*/
    
}
