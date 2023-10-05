using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStationsManager : MonoBehaviour
{

    [Header("STATIONS")]
    
    [SerializeField] private Transform stationTransform;
    
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float duration = 2.0f;
    
    [Space]
    
    [Header("TIME")]
    
    [SerializeField, Range(0, 10)] private float gameDurationSeconds;

    [Range(0, 1)]
    [SerializeField]
    private float _normalisedElapsedTime;

    private void Start()
    {
        _normalisedElapsedTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // continuously lerp the position of the stationTransform between the two vector3 positions with a given duration
        stationTransform.position = Vector3.Slerp(startPosition.position, targetPosition.position, Mathf.PingPong(Time.time, duration) / duration);
        // when the ping pong time is greater than the duration, the lerp will be reversed

        UpdateTime();
    }

    void UpdateTime()
    {
        _normalisedElapsedTime += Time.deltaTime/gameDurationSeconds;
        if (_normalisedElapsedTime >= gameDurationSeconds)
        {
            // end game
        }   
        AmbientManager.Instance.UpdateLight(_normalisedElapsedTime);
    }
}
