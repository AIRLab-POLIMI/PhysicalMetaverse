using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OdometryManager : Monosingleton<OdometryManager>
{
    //bool forward
    public bool _forward = false;
    //bool backward
    public bool _backward = false;
    //bool left
    public bool _left = false;
    //bool right
    public bool _right = false;
    //bool rotate left
    public bool _rotateLeft = false;
    //bool rotate right
    public bool _rotateRight = false;
    public GameObject _floor;
    //_personCollider
    public GameObject _personCollider;
    public float _forwardFloat = 0f;
    public float _rightFloat = 0f;
    public float _rotateRightFloat = 0f;

    public float _boolSpeed = 1f;
    public float _boolRotateSpeed = 1f;
    //for stations
    public float _speed = 1f;
    public bool _odometryActive = false;
    public float _odometryCooldown = 0.5f; //time required by the robot to halt after a movement
    private float _prevTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float _rotationSpeed = 1f;
    public float _movementSpeed = 1f;
    public float _odometryDeadzone = 0.2f;
    void FixedUpdate(){
        //move and rotate floor with speed
        if(_forward){
            _floor.transform.position += Vector3.forward * _boolSpeed * Time.deltaTime;
        }
        if(_backward){
            _floor.transform.position -= Vector3.forward * _boolSpeed * Time.deltaTime;
        }
        if(_left){
            _floor.transform.position -= Vector3.right * _boolSpeed * Time.deltaTime;
        }
        if(_right){
            _floor.transform.position += Vector3.right * _boolSpeed * Time.deltaTime;
        }
        if(_rotateLeft){
            _floor.transform.RotateAround(this.transform.position, Vector3.up, _boolRotateSpeed * Time.deltaTime);
            //rotate around 0 0 0 
            _personCollider.transform.RotateAround(Vector3.zero, Vector3.up, _boolRotateSpeed * Time.deltaTime);
        }
        if(_rotateRight){
            _floor.transform.RotateAround(this.transform.position, Vector3.up, -_boolRotateSpeed * Time.deltaTime);
            //rotate around 0 0 0
            _personCollider.transform.RotateAround(Vector3.zero, Vector3.up, -_boolRotateSpeed * Time.deltaTime);
        }
        //if(_forwardFloat > _odometryDeadzone){
        //abs value _forwardFloat
        float absForwardFloat = Mathf.Abs(_forwardFloat);

        if(absForwardFloat > _odometryDeadzone){
            //move floor forward
            _floor.transform.position -= Vector3.forward * _movementSpeed * Time.deltaTime * _forwardFloat;
        }
        float absRightFloat = Mathf.Abs(_rightFloat);
        if(absRightFloat > _odometryDeadzone){
            //move floor right
            _floor.transform.position += Vector3.right * _movementSpeed * Time.deltaTime * _rightFloat;
        }
        float absRotateRightFloat = Mathf.Abs(_rotateRightFloat);
        if(absRotateRightFloat > _odometryDeadzone){
            //rotate floor right around this transform
            _floor.transform.RotateAround(this.transform.position, Vector3.up, _rotationSpeed * Time.deltaTime * _rotateRightFloat);
        }
        //if any set _odometryActive
        if(_forward || _backward || _left || _right || _rotateLeft || _rotateRight || absForwardFloat > _odometryDeadzone || absRightFloat > _odometryDeadzone || absRotateRightFloat > _odometryDeadzone){
            _odometryActive = true;
            _prevTime = Time.time;

        }
        else{
            if(Time.time - _prevTime > _odometryCooldown)
                _odometryActive = false;
        }
    }

    public bool GetOdometryActive(){
        return _odometryActive;
    }
}
