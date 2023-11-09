using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PillarManager : MonoBehaviour
{
    //distance
    [SerializeField] private float _distance = 1f;
    //_disableColliderDistance
    [SerializeField] private float _disableColliderDistance = 1f;
    [SerializeField] private bool _blobEnabled = true;
    [SerializeField] private bool _disableMesh = true;
    private bool _prevCollided = false;
    [SerializeField] private int _pillarId = 0;
    [SerializeField] private int _stationId = 0;
    //bool _personColliding
    [SerializeField] private bool _personColliding = false;
    //bool _movementDetected
    [SerializeField] private bool _movementDetected = false;
    //_personTrackingWeight
    [SerializeField] private int _personTrackingWeight = 0;
    //_personWeight
    [SerializeField] private int _personWeight = 2;
    //_movementWeight
    [SerializeField] private int _movementWeight = 3;
    //_DISABLE_MOVEMENT_WITH_ODOMETRY
    [SerializeField] private bool _DISABLE_MOVEMENT_WITH_ODOMETRY = false;
    //serializefield alternate material
    [SerializeField] private Material _alternateMaterial = null;
    //serializefield bool debug material
    [SerializeField] private bool _debugMaterial = false;
    //private original material
    private Material _originalMaterial = null;
    //private movement detection material serialize
    [SerializeField] private Material _movementDetectionMaterial = null;
    [SerializeField] private Material _personTrackingMaterial = null;
    //_StationTrackingMaterial
    [SerializeField] private Material _StationTrackingMaterial = null;

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
    [SerializeField] private int _materialRestoreDeltaFrame = 2;
    [SerializeField] private int _latestDisableMeshFrame = 0;
    private PoseReceiver _poseReceiver;
    // Start is called before the first frame update
    void Start()
    {
        _poseReceiver = PoseReceiver.Instance;
        //if debug material is true
        if(_debugMaterial){
            //set material to alternate material
            GetComponent<Renderer>().material = _alternateMaterial;
            _originalMaterial = _alternateMaterial;
        }
    }

    //void setdistance
    public void SetDistance(float distance){
        if(distance > 100f){
            distance = 0;
        }
        _distance = distance;
    }

    //set person weight
    public void SetPersonWeight(int weight){
        _personWeight = weight;
    }

    //set movement weight
    public void SetMovementWeight(int weight){
        _movementWeight = weight;
    }

    [SerializeField] private int _poseWeight = 0;
    //set _poseWeight
    public void SetPoseWeight(int weight){
        _poseWeight = weight;
    }

    public void SetPillarId(int id){
        _pillarId = id;
        //ad id to end of name
        gameObject.name += id.ToString();
    }
    
    public float _relativeDistance = 0f;
    public float _movementTrackingThresholdClose = 1.7f;
    // Update is called once per frame
    void FixedUpdate()
    {
        //if distance is less than _disableColliderDistance disable collider, else enable it
        if(_distance < _disableColliderDistance){
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            GetComponent<Collider>().enabled = true;
        }
        GetPersonTrackingWeight();
        //if frame count is even
        if(Time.frameCount % 2 == 0){
            ResetValues();
        }
        if(Time.frameCount - _trackingFrame > _materialRestoreDeltaFrame){
            GetComponent<Renderer>().material = _alternateMaterial;
        }
        float _currentPositionMagnitude = Vector3.Magnitude(transform.position);
        float _prevPositionMagnitude = Vector3.Magnitude(_prevPosition);
        float relativeDistance = (float)Math.Max(_currentPositionMagnitude, _prevPositionMagnitude) / (float)Math.Min(_currentPositionMagnitude, _prevPositionMagnitude);
        //if magnitude of difference between prevposition and current position is greater than _movementTrackingThreshold set material to _movementDetectionMaterial
        //if(Vector3.Magnitude(transform.position - _prevPosition) > _movementTrackingThreshold){
        //_relativeDistance = relativeDistance;
        if(relativeDistance > _movementTrackingThreshold){
            //set material to _movementDetectionMaterial
            if(!OdometryManager.Instance.GetOdometryActive() || !_DISABLE_MOVEMENT_WITH_ODOMETRY){//&& _currentPositionMagnitude < _prevPositionMagnitude){
                if(_debugMaterial)
                    GetComponent<Renderer>().material = _movementDetectionMaterial;
                //if all OdometryManager instance is false set this station id to 9
                _stationId = 9;
                //movement detected
                _movementDetected = true;
                LidarManager.Instance.SetPersonBlobAt(_pillarId, GetPersonTrackingWeight());
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

        //if _latestDisableMeshFrame is current frame disable mesh, else enable it
        /*
        if(Time.frameCount - _latestDisableMeshFrame > 5){
            GetComponent<MeshRenderer>().enabled = true;
        }else{  
            GetComponent<MeshRenderer>().enabled = false;
        }
        */
        
    }

    private void ResetValues(){
        _stationId = -1;
        _movementDetected = false;
        _personColliding = false;
        _poseDetected = false;
        LidarManager.Instance.SetBlobAt(_pillarId, -1);
        LidarManager.Instance.SetPersonBlobAt(_pillarId, 0);
        //set collisions to false
        _collisionPillarLeft = false;
        _collisionPillarRight = false;
    }

    private int _poseConfirmationWeight = 0;
    //serialize bool _poseDetected
    [SerializeField] private bool _poseDetected = false;
    public int GetPersonTrackingWeight(){
        _personTrackingWeight = 0;
        if(_poseDetected){
            _poseConfirmationWeight = _poseWeight;
        }
        else{
            _poseConfirmationWeight = 0;
        }
        //if _personColliding return 1
        if(_personColliding){
            _personTrackingWeight = _personWeight + _poseConfirmationWeight;
            return _personTrackingWeight;// * (int)_distance;
        }
        //if _movementDetected return 2
        if(_movementDetected){
            _personTrackingWeight = _movementWeight + _poseConfirmationWeight + _poseConfirmationWeight/2;
            return _personTrackingWeight;// * (int)_distance;
        }
        return _personTrackingWeight + _poseConfirmationWeight;// * (int)_distance;
    }

    void OnTriggerStay(Collider other){
        if(!_blobEnabled){
            return;
        }
        //if colliding with "Station" color this gameobject in red
        if(other.gameObject.CompareTag("Station")){
            _trackingFrame = Time.frameCount;
            if(_debugMaterial)
                GetComponent<Renderer>().material = _StationTrackingMaterial;
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
        
        if(other.gameObject.CompareTag("Person") && _stationId<0){
            //_latestDisableMeshFrame = Time.frameCount;
            //disable mesh
            //GetComponent<MeshRenderer>().enabled = false;
            //set station id to 9
            _stationId = 9;
            _personColliding = true;
            LidarManager.Instance.SetPersonBlobAt(_pillarId, GetPersonTrackingWeight());
            if(_debugMaterial)
                GetComponent<Renderer>().material = _personTrackingMaterial;
            //set y to _personPillarDown
            //transform.position = new Vector3(transform.position.x, _personPillarDown, transform.position.z);
            //lerp
            //transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, _personPillarDown, transform.position.z), _pillarLerpSpeed);
            //TODO CHANGE HERE FOR PILLAR LERP
        }

        //if tag PCA _poseDetected
        if(other.gameObject.CompareTag("PCA")){
            _poseDetected = true;
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
