using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseManager : MonoBehaviour
{
    [SerializeField] private PoseReceiver _poseReceiver;
    //serialize field: _quantityOfMovement, _headAngleX, _headAngleY, _poseJoints scriptableobject, _distanceFromCamera
    //dictionary of all joints
    private Dictionary<string, Transform> _poseJoints = new Dictionary<string, Transform>();
    [SerializeField] private float _distanceFromCamera;
    [SerializeField] private float _headAngleX;
    [SerializeField] private float _headAngleY;
    [SerializeField] private float _quantityOfMovement;

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

    private void CalculateDistanceFromCamera()
    {
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


}
