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
    private NetworkingManager _networkingManager;
    // Start is called before the first frame update
    void Start()
    {
        _networkingManager = NetworkingManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float _rotationSpeed = 1f;
    public float _movementSpeed = 1f;
    public float _odometryDeadzone = 0.2f;
    private bool _analogTouched = false;
    void FixedUpdate(){
        _analogTouched = false;
        //if(_forwardFloat > _odometryDeadzone){
        //abs value _forwardFloat
        float absForwardFloat = Mathf.Abs(_forwardFloat);

        if(absForwardFloat > _odometryDeadzone){
            //move floor forward
            _floor.transform.position -= Vector3.forward * _movementSpeed * Time.deltaTime * _forwardFloat;
            //move personcollider
            _personCollider.transform.position -= Vector3.forward * _movementSpeed * Time.deltaTime * _forwardFloat;
            _analogTouched = true;
        }
        float absRightFloat = Mathf.Abs(_rightFloat);
        if(absRightFloat > _odometryDeadzone){
            //move floor right
            _floor.transform.position += Vector3.right * _movementSpeed * Time.deltaTime * _rightFloat;
            //rotate around 0 0 0
            _personCollider.transform.RotateAround(Vector3.zero, Vector3.up, _rotationSpeed * Time.deltaTime * _rightFloat);
            _analogTouched = true;
        }
        float absRotateRightFloat = Mathf.Abs(_rotateRightFloat);
        if(absRotateRightFloat > _odometryDeadzone){
            //rotate floor right around this transform
            _floor.transform.RotateAround(this.transform.position, Vector3.up, _rotationSpeed * Time.deltaTime * _rotateRightFloat);
            //rotate around 0 0 0
            _personCollider.transform.RotateAround(Vector3.zero, Vector3.up, _rotationSpeed * Time.deltaTime * _rotateRightFloat);
            _analogTouched = true;
        }
        //move and rotate floor with speed
        if(_forward){
            _floor.transform.position += Vector3.forward * _boolSpeed * Time.deltaTime;
            //move personcollider
            _personCollider.transform.position += Vector3.forward * _boolSpeed * Time.deltaTime;
            //_networkingManager send Ljy:255
            _networkingManager.SendString("Ljy:255","192.168.0.102");
        }
        else if(_backward){
            _floor.transform.position -= Vector3.forward * _boolSpeed * Time.deltaTime;
            //move personcollider
            _personCollider.transform.position -= Vector3.forward * _boolSpeed * Time.deltaTime;
            _networkingManager.SendString("Ljy:0","192.168.0.102");
        }
        else{
            
            if(!_analogTouched)
                _networkingManager.SendString("Ljy:127","192.168.0.102");
        }
        if(_left){
            _floor.transform.position -= Vector3.right * _boolSpeed * Time.deltaTime;
            //move personcollider
            _personCollider.transform.position -= Vector3.right * _boolSpeed * Time.deltaTime;
        }
        if(_right){
            _floor.transform.position += Vector3.right * _boolSpeed * Time.deltaTime;
            //move personcollider
            _personCollider.transform.position += Vector3.right * _boolSpeed * Time.deltaTime;
        }
        if(_rotateLeft){
            _floor.transform.RotateAround(this.transform.position, Vector3.up, _boolRotateSpeed * Time.deltaTime);
            //rotate around 0 0 0 
            _personCollider.transform.RotateAround(Vector3.zero, Vector3.up, _boolRotateSpeed * Time.deltaTime);
            _networkingManager.SendString("Ljx:0","192.168.0.102");
        }
        else if(_rotateRight){
            _floor.transform.RotateAround(this.transform.position, Vector3.up, -_boolRotateSpeed * Time.deltaTime);
            //rotate around 0 0 0
            _personCollider.transform.RotateAround(Vector3.zero, Vector3.up, -_boolRotateSpeed * Time.deltaTime);
            _networkingManager.SendString("Ljx:255","192.168.0.102");
        }
        else{
            
            if(!_analogTouched)
                _networkingManager.SendString("Ljx:127","192.168.0.102");
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
