using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityRepresentation : MonoBehaviour
{
    [SerializeField] private FloatSO LookingAt;

    [SerializeField] private GameObject _entityPrefab;

    [SerializeField] private FloatSO DistanceFromCenter;

    [SerializeField] private FloatSO QtyOfMovement;

    private GameObject _entity;
    
    [SerializeField] private DoubleFloatSO[] poseKeypoins;

    private float speed = 1f;
    private float EyeRotationSpeed = 1f;
    private float TentacleSpeed = 1f;
    
    private const int cameraDegrees = 64;
    
    private Vector3 defaultPosition = new Vector3(0f, 0.72f, 4.7f);
    private Vector3 defaultRotation = new Vector3(0f, 180f, 0f);

    private Vector3 defaultHidePosition = new Vector3(0f, -20f, 0f);

    private float newRotation;
    
    private Vector3 newPosition;

    private GameObject _eye;
    private GameObject _tentacleBase;

    private void Start()
    {
        SpawnEntity();
    }

    private void SpawnEntity()
    {
        GameObject obj = Instantiate(_entityPrefab, defaultHidePosition, Quaternion.Euler(0,180,0));
        _entity = obj;
        //_entity.transform.position = defaultPosition;
        //_entity.transform.rotation = Quaternion.Euler(defaultRotation);

        _eye = _entity.transform.GetChild(2).gameObject;
        _tentacleBase = _entity.transform.GetChild(0).gameObject;
        _entity.SetActive(false);
    }

    public void UpdateRep()
    {
        newRotation = LinearConversion.LinearConversionFloat(LookingAt.runtimeValue, 0f, 1f, -30f, 30f);
        
        //_entity.transform.rotation = Quaternion.Euler(0f, new_rotation, 90f);

        //_entity.transform.position = new Vector3(0f, 0.72f, DistanceFromCenter.runtimeValue); 
        
        //IF NECK IS PRESENT, PLACE THE ENTITY IN SCENE

        if (poseKeypoins[0].runtimeValue1 == -1)
        {
            //_entity.transform.position = defaultHidePosition;
            _entity.SetActive(false);
        }
        else
        {
            _entity.SetActive(true);
            float circleposition = (Mathf.Lerp(0f, (float) cameraDegrees, poseKeypoins[0].runtimeValue2) - 32f) / 360f;
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * (DistanceFromCenter.runtimeValue - 1.0f);
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * (DistanceFromCenter.runtimeValue - 1.0f);

            //_entity.transform.position = new Vector3(x + 0.5f, 1, z);
            newPosition = new Vector3(x + 0.5f, 1, z);
            
        }
    }

    public void Update()
    {
        float distance = Vector3.Distance(_entity.transform.position, newPosition);

        if (distance > 0.01f)
        {
            Vector3 setPosition =
                Vector3.Slerp(_entity.transform.position, newPosition, Time.deltaTime * EyeRotationSpeed);
            _entity.transform.position = setPosition;
        }
        

        //CHANGE EYE ROTATION
        
        Quaternion targetRotation = Quaternion.Euler(0f, newRotation, 0f);
        

        float rotationAngle = Quaternion.Angle(_eye.transform.localRotation, targetRotation);

        if (rotationAngle > 0.01f)
        {
            Quaternion newRotation =
                Quaternion.Slerp(_eye.transform.localRotation, targetRotation, EyeRotationSpeed * Time.deltaTime);


            _eye.transform.localRotation = newRotation;
        }
        
        //MOVE THE TENTACLES

        float offset = Mathf.PingPong(Time.time * QtyOfMovement.runtimeValue, 8) - 4f;
        
        _tentacleBase.transform.localRotation = Quaternion.Euler(offset, offset,offset);
    }
}
