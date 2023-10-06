using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OdometryManager : MonoBehaviour
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
    public float _forwardFloat = 0f;
    public float _rightFloat = 0f;
    public float _rotateRightFloat = 0f;

    //slider
    [Range(0.1f, 10f)]
    public float _speed = 1f;
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
            _floor.transform.localPosition += _floor.transform.forward * _movementSpeed * Time.deltaTime;
        }
        if(_backward){
            _floor.transform.localPosition -= _floor.transform.forward * _movementSpeed * Time.deltaTime;
        }
        if(_left){
            _floor.transform.localPosition -= _floor.transform.right * _movementSpeed * Time.deltaTime;
        }
        if(_right){
            _floor.transform.localPosition += _floor.transform.right * _movementSpeed * Time.deltaTime;
        }
        if(_rotateLeft){
            _floor.transform.Rotate(Vector3.up * _rotationSpeed * Time.deltaTime);
        }
        if(_rotateRight){
            _floor.transform.Rotate(Vector3.up * -_rotationSpeed * Time.deltaTime);
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
        
    }
}
