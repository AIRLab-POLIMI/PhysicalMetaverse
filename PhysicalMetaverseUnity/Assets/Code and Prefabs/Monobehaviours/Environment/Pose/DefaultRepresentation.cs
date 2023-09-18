using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultRepresentation : MonoBehaviour
{
    private const int arraySize = 18;

    private GameObject[] _points;

    [SerializeField] private DoubleFloatSO[] poseKeypoins;

    private Vector3 defaultHidePosition = new Vector3(-100, -100, -100);

    [SerializeField] private GameObject posePoint;

    private int cameraDegrees = 64;
    
    [SerializeField] private FloatSO distanceFromCenter;
    
    private float scaling = 5.0f;
    
    private float minDistValue = 1.0f;
    private float maxDistValue = 20.0f;

    private int minMeasure = 0;
    private int maxMeasure = 6000;

    private void Start()
    {
        _points = new GameObject[arraySize];

        SpawnPoints();
    }

    private void SpawnPoints()
    {
        for (int i = 0; i < arraySize; i++)
        {
            GameObject obj = Instantiate(posePoint, Vector3.zero, Quaternion.identity);
            obj.transform.parent = transform;
            _points[i] = obj;
            
            //Move object out of sight, as moving is faster than spawning/despawning and activating/deactivating
            obj.transform.position = defaultHidePosition;
        }
    }

    public void UpdateRep()
    {
        for (int i = 0; i < arraySize; i++)
        {
            if (poseKeypoins[i].runtimeValue1 == -1.0f)
            {
                _points[i].transform.position = defaultHidePosition;
            }
            else
            {
                float circleposition = (Mathf.Lerp(0f, (float) cameraDegrees, poseKeypoins[i].runtimeValue2) - 32f) / 360f;
                float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * distanceFromCenter.runtimeValue;
                float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * distanceFromCenter.runtimeValue;
            
                _points[i].transform.position =
                    new Vector3(x, (1 - poseKeypoins[i].runtimeValue1) * scaling, z); 
            }
            
        }
    }
    
    private float ConvertRange(int oldValue)
    {
        float newValue = ((((float) oldValue - (float) minMeasure) * (maxDistValue - minDistValue)) /
                          (maxMeasure - minMeasure)) + minDistValue;
        return newValue;
    }
}
