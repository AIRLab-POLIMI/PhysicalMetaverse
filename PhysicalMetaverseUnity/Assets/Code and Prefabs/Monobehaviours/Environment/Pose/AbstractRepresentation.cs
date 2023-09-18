using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractRepresentation : MonoBehaviour
{
    [SerializeField] private IntSO MaxConvertedAngle;
    [SerializeField] private IntSO MinConvertedAngle;
    
    [SerializeField] private FloatSO LookingAt;

    [SerializeField] private FloatSO QtyOfMovement;
    
    [SerializeField] private FloatSO distanceFromCenter;

    [SerializeField] private GameObject _pointPrefab;

    [SerializeField] private float timeMultiplier = 1f;

    private GameObject[] _points;
    
    private int arraySize = 64;

    private float startY;

    private Vector3[] initialPositions;

    //TESTING
    private float amplitude = 0f;
    private float frequency = 0f;
    private float speed = 1f;
    [SerializeField] private float offset = 0f;
    private float targetOffset;
    private float swarmDimension = 1f;
    private float colorSlider = 0f;

    private Color color1 = Color.gray;
    private Color color3 = Color.magenta;
    private Color color2 = new Color(255, 165, 0);
    private void Start()
    {
        _points = new GameObject[arraySize];
        SpawnPoints();
    }

    private void SpawnPoints()
    {
        float radius = 50f;
        
        
        
        startY = 20.0f;

        for (int i = 0; i < arraySize; ++i)
        {
            float circleposition = ((float)i / (float)360) - (4.0f/45.0f);
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * radius;
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * radius;
            GameObject obj = Instantiate(_pointPrefab, new Vector3(x, startY, z), Quaternion.identity);
            //GameObject obj = Instantiate(lidarPoint, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.LookRotation(new Vector3(x, 0.0f, z)));
            obj.transform.parent = transform;
            obj.GetComponent<Renderer>().material.color = color1;
            //obj.transform.position += obj.transform.forward*8.0f;
            _points[i] = obj;
            
        }
        
    }

    private void Update()
    {
        //PARAMETERS:
        //-DistanceFromCenter 
        //-QtyOfMovement-> 0 to 0.3 circa
        //-LookingAt-> 0 to 1
        //-Pose Midpoint -> 0 to max converted angle (circa 60)
        /*
        float amplitude = 20f - (1f * distanceFromCenter.runtimeValue);    // the amplitude of the sine wave
        float frequency = 1f + 5f * (QtyOfMovement.runtimeValue);    // the frequency of the sine wave
        float speed = 1f + (QtyOfMovement.runtimeValue * 10f);        // the speed at which the object moves up and down
        float lookingAtElaborated = 1 - Math.Abs(LookingAt.runtimeValue - 0.5f);
        */
        int poseMidpoint = -1;
        
        if(MinConvertedAngle.runtimeValue != -1)
            poseMidpoint = (MaxConvertedAngle.runtimeValue + MinConvertedAngle.runtimeValue) / 2;


        if (poseMidpoint == -1)
        {
            targetOffset = 0f;
            colorSlider = 0.0f;
        }
        else
        {
            targetOffset = GetDistanceRatio(poseMidpoint, 32, 32)/2;
            colorSlider = LookingAt.runtimeValue;
        }

        offset = Mathf.Lerp(offset, targetOffset, Time.deltaTime / 2);
        

        float targetAmplitude = 20f - distanceFromCenter.runtimeValue;
        
        
        
        amplitude = Mathf.Lerp(amplitude, targetAmplitude, Time.deltaTime * speed);
        
        swarmDimension = QtyOfMovement.runtimeValue * 5;



        for (int i = 0; i < arraySize; i++)
        {
            /*
            if (poseMidpoint != -1)
            {
                _points[i].transform.localScale = new Vector3(1, 0.5f + 8 * Mathf.Pow(GetDistanceRatio(i, poseMidpoint, 63), 10f) * (5 * lookingAtElaborated), 1);
            }
            else
            {
                _points[i].transform.localScale = new Vector3(1, 1, 1);
            }
            */
            /*
            float y = startY + amplitude * Mathf.Sin((Time.time*timeMultiplier)+((float)i/10)*2);

            _points[i].transform.position =
                new Vector3(_points[i].transform.position.x, y, _points[i].transform.position.z);
                */

            _points[i].transform.position = new Vector3(_points[i].transform.position.x, startY + amplitude * Mathf.Sin( Time.time + i * offset), _points[i].transform.position.z);
            _points[i].transform.localScale = new Vector3(1, 1 + swarmDimension, 1);
            _points[i].GetComponent<Renderer>().material.color = LerpColor(color1, color3, colorSlider);

        }
    }
    
    public static Color LerpColor(Color color1, Color color2, float t)
    {
        t = Mathf.Clamp01(t); // ensure t is within the range of 0 to 1
        return Color.Lerp(color1, color2, t);
    }


    private float GetDistanceRatio(int value1, int value2, int maxDistance)
    {
        float distance = Math.Abs(value1 - value2);

        return 1f - (distance / maxDistance);
    }
}
