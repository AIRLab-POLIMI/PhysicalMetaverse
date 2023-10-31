using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseManager : Monosingleton<PoseManager>
{
    [SerializeField] private PoseReceiver _poseReceiver;
    [SerializeField] private Transform _rotoTraslation;
    //_camera
    [SerializeField] private Transform _camera;
    [SerializeField] private GameObject _pose;
    //serialize field: _quantityOfMovement, _headAngleX, _headAngleY, _poseJoints scriptableobject, _distanceFromCamera
    //dictionary of all joints
    private Dictionary<string, Transform> _poseJoints = new Dictionary<string, Transform>();
    [SerializeField] private float _distanceFromCamera;
    [SerializeField] private float _headAngleX;
    //headTilt dof
    [SerializeField] private DOFController _headTilt;
    [SerializeField] private float _headAngleY;
    //headPan dof
    [SerializeField] private DOFController _headPan;
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
        return _headTilt.GetAngle();
    }

    public float GetHeadAngleY()
    {
        return _headPan.GetAngle();
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
        CalculateHeadAngles();
        CalculateQuantityOfMovement();
        CalculatePersonDetected();
        CalculateRotoTraslation();
        CalculateParallax();
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
                if(_zDistanceList.Count > _list_length) //check here if distance broke
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

    //float nose height
    private float _noseHeight = 0.8f;
    [SerializeField] private float _headXOffset = -45f;
    private void CalculateHeadAngles()
    {
        Quaternion bodyRotation = Quaternion.LookRotation(YDirection(_poseJoints["Left Shoulder"], _poseJoints["Right Shoulder"])) * Quaternion.Euler(0, 0, 0);
        //final angle should be x=0, y=direction, z=-90
        Quaternion leftAngle = Quaternion.LookRotation(YDirection(_poseJoints["Left Ear"], _poseJoints["Left Eye"]));
        Quaternion rightAngle = Quaternion.LookRotation(YDirection(_poseJoints["Right Ear"], _poseJoints["Right Eye"]));
        float avgAngle = (leftAngle.eulerAngles.y + rightAngle.eulerAngles.y) / 2 - bodyRotation.eulerAngles.y;
        _headAngleY = avgAngle;
        _headPan.SetAngle(_headAngleY);
        //set VCamPan to eyeDirection
        //_odileJoints["VCamPan"].localRotation = headPan;
        //Quaternion headTilt = Quaternion.LookRotation(ZDirection(_poseJoints["Left Ear"], _poseJoints["Nose"])) * Quaternion.Euler(90, 0, 0);
        Quaternion leftTilt = Quaternion.LookRotation(XDirection(_poseJoints["Left Ear"], _poseJoints["Nose"]));
        Quaternion rightTilt = Quaternion.LookRotation(XDirection(_poseJoints["Right Ear"], _poseJoints["Nose"]));
        avgAngle = (leftTilt.eulerAngles.x + rightTilt.eulerAngles.x) / 2;
        _noseHeight = _poseJoints["Nose"].localPosition.y;
        //avg angle * tiltzerp / filtered distance
        //avgAngle = avgAngle * _tiltZeroDistance / _oldZDistance;
        //avgAngle = avgAngle + (_noseHeight - _tiltZeroHeight) * _noseHeightMultiplier; //TODO
        //set VCamTilt to headTilt
        _headAngleX = -avgAngle - _headXOffset;
        _headTilt.SetAngle(_headAngleX);
    }
    
    //return the vector ortogonal to two vectors on the yz plane
    Vector3 XDirection(Transform a, Transform b)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 aPosXZ = new Vector3(0, aPos.y, aPos.z);
        Vector3 bPosXZ = new Vector3(0, bPos.y, bPos.z);
        Vector3 aToB = bPosXZ - aPosXZ;
        Vector3 aToBOrtho = new Vector3(0, -aToB.z, aToB.y);
        return aToBOrtho;
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
    private Vector3 YDirection(Transform a, Transform b)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 aPosXZ = new Vector3(aPos.x, 0, aPos.z);
        Vector3 bPosXZ = new Vector3(bPos.x, 0, bPos.z);
        Vector3 aToB = bPosXZ - aPosXZ;
        Vector3 aToBOrtho = new Vector3(-aToB.z, 0, aToB.x);
        return aToBOrtho;
    }

    public float _camAngleSensitivity = 10f;
    public float _camXAngleSensitivity = 10f;
    [SerializeField] private float _lerpNose;
    private void CalculateParallax(){
        //Parallax stuff

        //map localtransform x from -5,5 to 90,270 and set rotation of _camera
        //clamp from -5 to 5
        float camYAngle = Mathf.Clamp(_rotoTraslation.localPosition.x, -5f, 5f);
        camYAngle = -(_rotoTraslation.localPosition.x/5f) * _camAngleSensitivity;// + 180f;
        //lerp _lerpNose to nose
        _lerpNose = Mathf.Lerp(_lerpNose, _poseJoints["Nose"].localPosition.y, _lerpSpeed);
        //camXAngle add 1 and clamp between -2 and 2
        float camXAngle = Mathf.Clamp(_lerpNose + 1f, -2f, 2f);
        
        camXAngle = (camXAngle/2f) * _camXAngleSensitivity;

        if(_personDetected){
            _camera.localRotation = Quaternion.Lerp(_camera.localRotation, Quaternion.Euler(camXAngle, camYAngle, 0), _lerpSpeed);
        }
        else{
            _camera.localRotation = Quaternion.Lerp(_camera.localRotation, Quaternion.Euler(0, 0, 0), _lerpSpeed);
        }
    }
}
