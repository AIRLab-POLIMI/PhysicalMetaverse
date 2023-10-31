using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseManager : Monosingleton<PoseManager>
{
    [SerializeField] private PoseReceiver _poseReceiver;
    [SerializeField] private Transform _rotoTraslation;
    [SerializeField] private GameObject _pose;
    //serialize field: _quantityOfMovement, _headAngleX, _headAngleY, _poseJoints scriptableobject, _distanceFromCamera
    //dictionary of all joints
    private Dictionary<string, Transform> _poseJoints = new Dictionary<string, Transform>();
    [SerializeField] private float _distanceFromCamera;
    [SerializeField] private float _headAngleX;
    [SerializeField] private float _headAngleY;
    [SerializeField] private float _quantityOfMovement;
    [SerializeField] private bool _personDetected = false;

    //
    [SerializeField] private  bool _MANUAL_MOVEMENT = false;

    public Dictionary<string, Transform> GetPoseJoints()
    {
        return _poseJoints;
    }

    public float GetDistanceFromCamera()
    {
        return _distanceFromCamera;
    }

    public float GetHeadAngleX()
    {
        return _headAngleX;
    }

    public float GetHeadAngleY()
    {
        return _headAngleY;
    }

    public float GetQuantityOfMovement()
    {
        return _quantityOfMovement;
    }

    public bool GetPersonDetected()
    {
        return _personDetected;
    }

    public Transform GetRotoTraslation()
    {
        return _rotoTraslation;
    }

    public bool GetManualMovement()
    {
        return _MANUAL_MOVEMENT;
    }

    // Start is called before the first frame update
    void Start()
    {
        _poseReceiver = PoseReceiver.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        CalculatePoseJoints();
        CalculateDistanceFromCamera();
        CalculateHeadAngleX();
        CalculateHeadAngleY();
        CalculateQuantityOfMovement();
        CalculatePersonDetected();
        CalculateRotoTraslation();
    }

    private bool _notPopulated = true;
    private void CalculatePoseJoints()
    {
        //if spawned
        if (_notPopulated)
        {
            if (_poseReceiver.GetSpawned()){
                _poseJoints.Clear();
                //print names of all spheres
                foreach (GameObject sphere in _poseReceiver.GetSpheres())
                {
                    //print(sphere.name);
                    _poseJoints.Add(sphere.name, sphere.transform);
                }
                _notPopulated = false;
            }
            else
                return;
        }
    }

    private float _oldZDistance = 0.7f;
    private float _distanceDeltaTolerance = 1f;
    private List<float> _zDistanceList = new List<float>();
    //static _list_length
    private int _list_length = 20;
    private void CalculateDistanceFromCamera()
    {
        float zDistance = _poseReceiver.GetDistance();
        //if zDistance changed more than 100% of oldZDistance use oldZDistance
        if(Mathf.Abs(zDistance - _oldZDistance) > _distanceDeltaTolerance)
            zDistance = _oldZDistance;
        else
            _zDistanceList.Add(zDistance);
            if(_zDistanceList.Count > _list_length)
                _zDistanceList.RemoveAt(0);
                //sort list
                //sorted list
                List<float> sortedList = new List<float>();
                //for each value in _zDistanceList
                foreach (float value in _zDistanceList)
                {
                    //add value to sorted list
                    sortedList.Add(value);
                }
                //sort sorted list
                sortedList.Sort();
                //choose middle value
                zDistance = sortedList[_list_length/2];
                //zDistance = min of list
                /*for(int i = 0; i < _zDistanceList.Count; i++){
                    if(_zDistanceList[i] < zDistance)
                        zDistance = _zDistanceList[i];
                }*/
            _oldZDistance = zDistance;

        if(!_personDetected){
            _oldZDistance = 0.6f;
        }

        _distanceFromCamera = _oldZDistance;
    }

    private void CalculateHeadAngleX()
    {
    }

    private void CalculateHeadAngleY()
    {
    }

    private void CalculateQuantityOfMovement()
    {
    }

    [SerializeField] private float _exitX = 5.9f;
    private void CalculatePersonDetected()
    {
        //if abs x less than _exitX
        if(Mathf.Abs(_rotoTraslation.localPosition.x) < _exitX){
            _personDetected = true;
        }
        else{
            _personDetected = false;
        }
    }

    [SerializeField] private float _rotationOffset = 0f;
    public void SetRotationOffset(float rotationOffset){
        _rotationOffset = rotationOffset;
    }
    [SerializeField] private float _lerpSpeed = 0.1f;
    [SerializeField] private float _xTraslationMultiplier = 3.1f;
    [SerializeField] private float _yTraslationMultiplier = 0f;
    [SerializeField] private float _zTraslationMultiplier = 8.8f;
    //transform _offsetWithPose
    [SerializeField] private Vector3 _offsetWithPose = new Vector3(18.12f, 0, -9.9f);
    private void CalculateRotoTraslation()
    {
        //ROTATION
        //set transform.localrotation y angle from YDirection of left shoulder and right shoulder
        //transform.localRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"]));
        //quaternion destRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"])) + 180 on y
        Quaternion destRotation = Quaternion.LookRotation(YDirection(_poseJoints["Left Shoulder"], _poseJoints["Right Shoulder"])) * Quaternion.Euler(0, -_rotationOffset, 0);
        //rotate 180
        destRotation *= Quaternion.Euler(0, 180, 0);
        //lerp
        _rotoTraslation.localRotation = Quaternion.Lerp(_rotoTraslation.localRotation, destRotation, _lerpSpeed);


        //TRASLATION
        //pose location = (_joints["Left Shoulder"].localPosition + _joints["Right Shoulder"].localPosition) / 2;
        Vector3 poseLocation = Vector3.zero;
        Vector3 poseXLocation = (_poseJoints["Left Shoulder"].localPosition + _poseJoints["Right Shoulder"].localPosition) / 2;
        
        /*Vector3 poseZLocation = _joints["Left Shoulder"].localPosition - _joints["Left Ankle"].localPosition;

        float length_to_measure = poseZLocation.y;

        // Focal length of the camera in millimeters
        float focal_length_mm = 4.81f;
        float actual_square_size_meters = 1f;  // Replace this with the actual measurement

        // Calculate the angular size in radians
        float angular_size_rad = 2 * Mathf.Atan(length_to_measure / (2 * focal_length_mm));

        //# Calculate the distance using the formula
        float distance_meters = (actual_square_size_meters / 2) / Mathf.Tan(angular_size_rad / 2);
        distance_meters = distance_meters*100* 33/13;
        //#print("Distance from camera to square: {:.2f} meters".format(distance_meters))
        distance_meters *= 100;*/
        float xPosition = poseXLocation.x;
        //correct with zDistance
        //xPosition = (xPosition - 7f) * (zDistance/_perspectiveCorrection);
        poseLocation = new Vector3(xPosition, 0, _distanceFromCamera);
        //target = pose transform plus middle between left shoulder and right shoulder
        Vector3 target = _pose.transform.localPosition + new Vector3(_xTraslationMultiplier*poseLocation.x + _offsetWithPose.x, _yTraslationMultiplier*poseLocation.y + _offsetWithPose.y, _zTraslationMultiplier*poseLocation.z + _offsetWithPose.z);
        target.y = 0f;
        //lerp position to left foot
        if(!_MANUAL_MOVEMENT)
            _rotoTraslation.localPosition = Vector3.Lerp(_rotoTraslation.localPosition, target, _lerpSpeed);
    }

    //return the vector ortogonal to two vectors on the xz plane
    Vector3 YDirection(Transform a, Transform b)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 aPosXZ = new Vector3(aPos.x, 0, aPos.z);
        Vector3 bPosXZ = new Vector3(bPos.x, 0, bPos.z);
        Vector3 aToB = bPosXZ - aPosXZ;
        Vector3 aToBOrtho = new Vector3(-aToB.z, 0, aToB.x);
        return aToBOrtho;
    }

}
