using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseManager : Monosingleton<PoseManager>
{
    [SerializeField] private PoseReceiver _poseReceiver;
    [SerializeField] private Transform _rotoTraslation;
    //serialize field: _quantityOfMovement, _headAngleX, _headAngleY, _poseJoints scriptableobject, _distanceFromCamera
    //dictionary of all joints
    private Dictionary<string, Transform> _poseJoints = new Dictionary<string, Transform>();
    [SerializeField] private float _distanceFromCamera;
    [SerializeField] private float _headAngleX;
    [SerializeField] private float _headAngleY;
    [SerializeField] private float _quantityOfMovement;
    [SerializeField] private bool _PERSON_DETECTED = false;

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
        return _PERSON_DETECTED;
    }

    public Transform GetRotoTraslation()
    {
        return _rotoTraslation;
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

        if(_PERSON_DETECTED){
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

    private float _exitX = 5.9f;
    private void CalculatePersonDetected()
    {
        //if abs x less than _exitX
        if(Mathf.Abs(_rotoTraslation.localPosition.x) < _exitX){
            _PERSON_DETECTED = true;
        }
        else{
            _PERSON_DETECTED = false;
        }
    }

    private float _rotationOffset = 0f;
    public void SetRotationOffset(float rotationOffset){
        _rotationOffset = rotationOffset;
    }
    private void CalculateRotoTraslation()
    {
        //set transform.localrotation y angle from YDirection of left shoulder and right shoulder
        //transform.localRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"]));
        //quaternion destRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"])) + 180 on y
        Quaternion destRotation = Quaternion.LookRotation(YDirection(_poseJoints["Left Shoulder"], _poseJoints["Right Shoulder"])) * Quaternion.Euler(0, 180 - _rotationOffset, 0);
        //lerp
        _rotoTraslation.localRotation = Quaternion.Lerp(_rotoTraslation.localRotation, destRotation, 0.1f);
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
