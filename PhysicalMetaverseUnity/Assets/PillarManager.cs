using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PillarManager : MonoBehaviour
{
    [SerializeField] private bool _blobEnabled = true;
    [SerializeField] private bool _disableMesh = true;
    private bool _prevCollided = false;
    [SerializeField] private int _pillarId = 0;
    [SerializeField] private int _stationId = 0;
    //serializefield alternate material
    [SerializeField] private Material _alternateMaterial = null;
    //serializefield bool debug material
    [SerializeField] private bool _debugMaterial = false;
    //private original material
    private Material _originalMaterial = null;
    //private movement detection material serialize
    [SerializeField] private Material _movementDetectionMaterial = null;

    //collision pillar left bool
    [SerializeField] private bool _collisionPillarLeft = false;
    //collision pillar right bool
    [SerializeField] private bool _collisionPillarRight = false;
    //prev
    private bool _prevCollisionPillarLeft = false;
    //prev
    private bool _prevCollisionPillarRight = false;
    private Vector3 _prevPosition = Vector3.zero;
    [SerializeField] private float _movementTrackingThreshold = 1f;
    private int _trackingFrame = 0;
    [SerializeField] private int _materialRestoreDeltaFrame = 5;
    // Start is called before the first frame update
    void Start()
    {
        //if debug material is true
        if(_debugMaterial){
            //set material to alternate material
            GetComponent<Renderer>().material = _alternateMaterial;
            _originalMaterial = _alternateMaterial;
        }
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
            LidarManager.Instance.SetPersonBlobAt(_pillarId, -1);
            //set collisions to false
            _collisionPillarLeft = false;
            _collisionPillarRight = false;
        }
        if(Time.frameCount - _trackingFrame > _materialRestoreDeltaFrame){
            GetComponent<Renderer>().material = _alternateMaterial;
        }
        float _currentPositionMagnitude = Vector3.Magnitude(transform.position);
        float _prevPositionMagnitude = Vector3.Magnitude(_prevPosition);
        float relativeDistance = (float)Math.Max(_currentPositionMagnitude, _prevPositionMagnitude) / (float)Math.Min(_currentPositionMagnitude, _prevPositionMagnitude);
        //if magnitude of difference between prevposition and current position is greater than _movementTrackingThreshold set material to _movementDetectionMaterial
        //if(Vector3.Magnitude(transform.position - _prevPosition) > _movementTrackingThreshold){
        if(relativeDistance > _movementTrackingThreshold){
            //set material to _movementDetectionMaterial
            if(!OdometryManager.Instance.GetOdometryActive()){//&& _currentPositionMagnitude < _prevPositionMagnitude){
                if(_debugMaterial)
                    GetComponent<Renderer>().material = _movementDetectionMaterial;
                //if all OdometryManager instance is false set this station id to 9
                _stationId = 9;
                LidarManager.Instance.SetPersonBlobAt(_pillarId, _stationId);
            }
            //if(_currentPositionMagnitude > _prevPositionMagnitude){
            //    GetComponent<Renderer>().material = _alternateMaterial;
            //}
            //get current frame
            _trackingFrame = Time.frameCount;
            _prevPosition = transform.position;
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
                float alpha = singlestation.GetPetalsAlpha();
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
            ////GetComponent<MeshRenderer>().enabled = false;
            //set station id to 9
            _stationId = 9;
            LidarManager.Instance.SetPersonBlobAt(_pillarId, _stationId);
            if(_debugMaterial)
                GetComponent<Renderer>().material = _movementDetectionMaterial;
            //set y to _personPillarDown
            //transform.position = new Vector3(transform.position.x, _personPillarDown, transform.position.z);
            //lerp
            //transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, _personPillarDown, transform.position.z), _pillarLerpSpeed);
            //TODO CHANGE HERE FOR PILLAR LERP
        }

        //if collision with Pillar tag check if id greater or less than this and set left and right booleans
        /*if(other.gameObject.CompareTag("Pillar")){
            //GetPillarId
            int otherPillarId = other.gameObject.GetComponent<PillarManager>().GetPillarId();
            //if otherPillarId is greater than this pillar id
            if(otherPillarId > _pillarId){
                //set collision pillar right to true
                _collisionPillarRight = true;
            }else{
                //set collision pillar left to true
                _collisionPillarLeft = true;
            }
            //check with prev, if one differs set material to _alternateMaterial
            if(_prevCollisionPillarLeft != _collisionPillarLeft || _prevCollisionPillarRight != _collisionPillarRight){
                //set material to _alternateMaterial
                if(_debugMaterial){
                        GetComponent<Renderer>().material = _movementDetectionMaterial;
                }
            }
            _prevCollisionPillarLeft = _collisionPillarLeft;
            _prevCollisionPillarRight = _collisionPillarRight;

        }*/
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

    public void SetStationId(int id){
        _stationId = id;
    }

    public int GetPillarId(){
        return _pillarId;
    }

    public void UpdateBehaviour(float personPillarDown, float pillarLerpSpeed, float backUpReducer){
        _personPillarDown = personPillarDown;
        _pillarLerpSpeed = pillarLerpSpeed;
        _backUpReducer = backUpReducer;
    }
    
}
