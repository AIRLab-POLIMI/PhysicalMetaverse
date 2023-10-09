using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarManager : MonoBehaviour
{
    [SerializeField] private bool _blobEnabled = true;
    [SerializeField] private bool _disableMesh = true;
    private bool _prevCollided = false;
    [SerializeField] private int _pillarId = 0;
    [SerializeField] private int _stationId = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetPillarId(int id){
        _pillarId = id;
        //ad id to end of name
        gameObject.name += id.ToString();
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        //if frame count is even
        if(Time.frameCount % 2 == 0){
            _stationId = -1;
            LidarManager.Instance.SetBlobAt(_pillarId, -1);
        }
        //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        //lerp
        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0, transform.position.z), _pillarLerpSpeed/_backUpReducer);
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
            LidarManager.Instance.SetBlobAt(_pillarId, _stationId);
            //if other gameobject has SingleStationManager get alpha of its interaction object and set 1 - alpha to this gameobject
            SingleStationManager singlestation = other.gameObject.transform.parent.gameObject.GetComponent<SingleStationManager>();
            if(singlestation != null){
                //get alpha
                float alpha = singlestation._petalsAlpha;
                //set alpha
                GetComponent<Renderer>().material.color = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b, 1 - alpha);
                //singlestation.Show();
                //GetComponent<MeshRenderer>().enabled = true;
            }
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
        
        if(other.gameObject.CompareTag("Person")){
            //disable mesh
            GetComponent<MeshRenderer>().enabled = false;
            //set y to _personPillarDown
            //transform.position = new Vector3(transform.position.x, _personPillarDown, transform.position.z);
            //lerp
            //transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, _personPillarDown, transform.position.z), _pillarLerpSpeed);
            //TODO CHANGE HERE FOR PILLAR LERP
        }
    }

    [SerializeField] private float _personPillarDown = -3f;
    [SerializeField] private float _pillarLerpSpeed = 0.1f;
    [SerializeField] private float _backUpReducer = 3f;

    /*void OnTriggerExit(Collider other){
        //color this gameobject in yellow
        GetComponent<Renderer>().material.color = Color.yellow;
        //enable mesh
        GetComponent<MeshRenderer>().enabled = true;
    }*/
    public int GetStationId(){
        return _stationId;
    }

    public void UpdateBehaviour(float personPillarDown, float pillarLerpSpeed, float backUpReducer){
        _personPillarDown = personPillarDown;
        _pillarLerpSpeed = pillarLerpSpeed;
        _backUpReducer = backUpReducer;
    }
    
}
